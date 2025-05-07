Dropzone.autoDiscover = false;

// Lưu trữ ghi chú để có thể truy cập nhanh mà không cần gọi API
const noteStorage = new Map();
let isInitialized = false;

// Store current doctor info globally
let currentDoctor = null;

// Hàm lấy thông tin bác sĩ hiện tại
async function getCurrentDoctor() {
    try {
        const response = await fetch('/api/Auth/GetCurrentDoctor', {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Network response was not ok: ${response.status} - ${errorText}`);
        }

        const data = await response.json();
        if (data.success) {
            currentDoctor = {
                id: data.id,
                name: data.name,
                userId: data.userId
            };
            return currentDoctor;
        } else {
            throw new Error(data.message || 'Failed to get current doctor info');
        }
    } catch (error) {
        console.error('Error getting current doctor:', error);
        notyf.error('Không thể lấy thông tin bác sĩ hiện tại');
        throw error;
    }
}

// Đảm bảo thông tin bác sĩ được tải trước khi thực hiện các thao tác
async function ensureCurrentDoctor() {
    if (!currentDoctor) {
        await getCurrentDoctor();
    }
    return currentDoctor;
}

// Hàm hiển thị ghi chú toàn cục
function displayNote(note, type, identifier) {
    if (!note) {
        note = 'Không có ghi chú';
    }

    let title = '';
    if (type === 'treatment') {
        title = `Ghi chú điều trị - ${identifier}`;
    } else if (type === 'regulation') {
        title = `Ghi chú quy định - ${identifier}`;
    } else if (type === 'assignment') {
        title = `Ghi chú phân công - ${identifier}`;
    }

    $.confirm({
        title: title,
        content: note,
        type: 'blue',
        typeAnimated: true,
        theme: 'modern',
        boxWidth: '500px',
        useBootstrap: false,
        buttons: {
            close: {
                text: 'Đóng',
                btnClass: 'btn-default'
            }
        }
    });
}

function setupNoteButton(button, code, note) {
    // Lưu ghi chú vào storage
    noteStorage.set(code, note || '');

    // Xóa event cũ (nếu có) và thêm event mới
    button.off('click').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        displayNote(noteStorage.get(code), 'treatment', code);
    });

    // Cập nhật style của button
    if (note) {
        button.removeClass('btn-secondary').addClass('btn-info');
    } else {
        button.removeClass('btn-info').addClass('btn-secondary');
    }
}

document.addEventListener('alpine:init', () => {
    Alpine.data('editData', () => ({
        dropzone: null,
        tab: 'treatment-record',
        hasHealthInsurance: false,
        isRightRoute: false,
        showAddTreatmentForm: false,
        showAddAssignmentForm: false,
        regulations: [],
        allRegulations: window.regulationsData || [],
        isLoading: false,
        overlay: null,
        TreatmentRecordDetails: window.treatmentRecordDetailsData || [],
        Assignments: window.assignmentsData || [],
        currentEmployeeId: window.currentEmployeeId || '',

        // Sử dụng hàm displayNote toàn cục thông qua wrapper
        displayNote(note, type, identifier) {
            // Ngăn chặn sự kiện click từ Alpine nếu đã có event handler jQuery
            if (type === 'treatment' && noteStorage.has(identifier)) {
                return;
            }
            displayNote(note, type, identifier);
        },

        areTreatmentDatesSelected() {
            const startDateElement = document.getElementById('treatmentRecordStartDate');
            const endDateElement = document.getElementById('treatmentRecordEndDate');

            if (!startDateElement || !endDateElement) {
                console.warn('Treatment date elements not found');
                return false;
            }

            const startDate = startDateElement.value;
            const endDate = endDateElement.value;
            return startDate && endDate;
        },

        updateRegulationSelectsState() {
            const regulationSelects = document.querySelectorAll('[id^="regulationId-"]');
            const areDatesSelected = this.areTreatmentDatesSelected();

            regulationSelects.forEach(select => {
                if (!areDatesSelected) {
                    select.disabled = true;
                    select.classList.add('opacity-50', 'cursor-not-allowed');
                    // Show warning if parent container exists
                    const warningContainer = select.closest('.grid')?.querySelector('.text-sm');
                    if (warningContainer) {
                        warningContainer.innerHTML = `
                            <div class="flex items-center whitespace-nowrap">
                                <svg class="w-4 h-4 inline-block mr-1 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
                                    <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
                                </svg>
                                <span class="flex-shrink-0">Vui lòng chọn thời gian điều trị trước khi chọn quy định!</span>
                            </div>
                        `;
                        warningContainer.style.display = 'block';
                        warningContainer.style.color = 'coral';
                        warningContainer.style.fontWeight = 'bold';
                    }
                } else {
                    select.disabled = false;
                    select.classList.remove('opacity-50', 'cursor-not-allowed');
                    // Hide warning if parent container exists
                    const warningContainer = select.closest('.grid')?.querySelector('.text-sm');
                    if (warningContainer) {
                        warningContainer.style.display = 'none';
                    }
                }
            });
        },

        setupDateListeners() {
            const startDateInput = document.getElementById('treatmentRecordStartDate');
            const endDateInput = document.getElementById('treatmentRecordEndDate');

            const updateState = () => {
                this.updateRegulationSelectsState();
            };

            startDateInput.addEventListener('change', updateState);
            endDateInput.addEventListener('change', updateState);
        },

        async init() {
            if (isInitialized) return;

            // Lấy thông tin bác sĩ trước khi khởi tạo các thành phần khác
            await getCurrentDoctor();

            // Khởi tạo regulations từ initialRegulations nếu có
            if (window.initialRegulations && Array.isArray(window.initialRegulations) && window.initialRegulations.length > 0) {
                this.regulations = window.initialRegulations.map(r => {
                    // Tra cứu tên quy định từ allRegulations
                    let regulationName = '';
                    if (window.regulationsData && Array.isArray(window.regulationsData)) {
                        const found = window.regulationsData.find(item => item.id?.toString() === r.regulationId?.toString());
                        if (found) regulationName = found.name;
                    }
                    return {
                        Code: r.code,
                        RegulationId: r.regulationId,
                        RegulationName: regulationName,
                        ExecutionDate: r.executionDate ? this.formatVNDate(r.executionDate) : '',
                        Note: r.note || '',
                        isOld: true
                    };
                });
                await this.$nextTick();
            }


            // Nếu đã có regulations, tự động gán vào các input tương ứng cho từng dòng
            for (let i = 0; i < this.regulations.length; i++) {
                const reg = this.regulations[i];
                // Gán cho select
                const select = document.getElementById('regulationId-' + i);
                if (select && reg.RegulationId) {
                    select.value = reg.RegulationId;
                    // Nếu option chưa có, thêm option để hiển thị đúng tên
                    let found = false;
                    for (let j = 0; j < select.options.length; j++) {
                        if (select.options[j].value == reg.RegulationId) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        const opt = document.createElement('option');
                        opt.value = reg.RegulationId;
                        opt.text = reg.RegulationName || 'Đã chọn';
                        opt.selected = true;
                        select.appendChild(opt);
                    }
                }
                // Gán cho ngày thực hiện
                const dateInput = document.getElementById('executionDate-' + i);
                if (dateInput && reg.ExecutionDate) {
                    dateInput.value = reg.ExecutionDate;
                }
                // Gán cho ghi chú
                const noteInput = document.getElementById('note-' + i);
                if (noteInput && reg.Note) {
                    noteInput.value = reg.Note;
                }
            }

            this.setupChoices();
            this.setupFlatpickr();
            this.setupDropzone();
            this.setupValidation();
            this.setupDateListeners();
            this.updateRegulationSelectsState();

            // Setup health insurance checkbox
            const healthInsuranceCheckbox = document.querySelector('[name="Patient.HasHealthInsurance"]');
            if (healthInsuranceCheckbox) {
                // Initialize from data attribute
                this.hasHealthInsurance = healthInsuranceCheckbox.getAttribute('data-has-insurance') === 'true';
                healthInsuranceCheckbox.checked = this.hasHealthInsurance;

                // Add change event listener
                healthInsuranceCheckbox.addEventListener('change', (e) => {
                    this.hasHealthInsurance = e.target.checked;
                });
            }

            const healthInsuranceIsRightRoute = document.querySelector('[name="Patient.HealthInsuranceIsRightRoute"]')?.value;
            this.isRightRoute = !!(healthInsuranceIsRightRoute);

            $('select').on('focus', function () {
                $(this).trigger('click');
            });

            // Khởi tạo các nút xem ghi chú
            $('button[data-note]').each(function () {
                const button = $(this);
                const code = button.data('code');
                const note = button.data('note');
                setupNoteButton(button, code, note);
            });

            // Initialize regulations data from ViewBag
            this.allRegulations = window.regulationsData || [];

            // Khởi tạo overlay khi component được tạo
            this.overlay = document.getElementById('loadingOverlay');

            isInitialized = true;
        },

        // Thêm hàm formatVNDate để chuyển ngày ISO sang dd/MM/yyyy
        formatVNDate(dateStr) {
            if (!dateStr) return '';
            const d = new Date(dateStr);
            if (isNaN(d.getTime())) return '';
            const day = String(d.getDate()).padStart(2, '0');
            const month = String(d.getMonth() + 1).padStart(2, '0');
            const year = d.getFullYear();
            return `${day}/${month}/${year}`;
        },

        setupChoices() {
            // Khởi tạo choices cho tất cả select có class 'choices'
            const selectElements = document.querySelectorAll('.choices');
            selectElements.forEach(select => {
                // Skip if already initialized or is treatment method or room select
                if (select.id === 'treatmentRecordDetailRoom' ||
                    select.id === 'treatmentRecordDetailTreatmentMethod' ||
                    select.choices) return;

                new Choices(select, {
                    searchEnabled: true,
                    searchPlaceholderValue: 'Tìm kiếm...',
                    removeItemButton: true,
                    noResultsText: 'Không tìm thấy kết quả',
                    noChoicesText: 'Không có lựa chọn nào',
                    itemSelectText: ''
                });
            });

            this.initRoomChoices();
            this.initTreatmentMethodChoices();
        },

        initRoomChoices() {
            const roomSelect = document.getElementById('treatmentRecordDetailRoom');
            if (!roomSelect || roomSelect.choices) return; // Skip if already initialized

            if (this.roomChoices) {
                this.roomChoices.destroy();
            }

            this.roomChoices = new Choices(roomSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm phòng...',
                removeItemButton: true,
                noResultsText: 'Không tìm thấy phòng',
                noChoicesText: 'Không có phòng nào',
                itemSelectText: '',
                placeholder: true,
                placeholderValue: 'Chọn phòng'
            });

            roomSelect.choices = this.roomChoices; // Mark as initialized
            this.roomChoices.disable(); // Initially disable room select
        },

        initTreatmentMethodChoices() {
            const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');
            if (!treatmentMethodSelect || treatmentMethodSelect.choices) return;

            const treatmentMethodChoices = new Choices(treatmentMethodSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm phương pháp điều trị...',
                removeItemButton: false,
                noResultsText: 'Không tìm thấy kết quả',
                noChoicesText: 'Không có phương pháp điều trị nào',
                itemSelectText: '',
                allowHTML: true
            });

            // Disable the select since it's pre-filled with employee's treatment method
            treatmentMethodChoices.disable();
            treatmentMethodSelect.classList.add('opacity-75');
        },

        loadAvailableTreatmentMethods(treatmentMethodChoices) {
            const treatmentRecordId = document.getElementById('treatmentRecordId').value;
            const treatmentMethodWarning = document.getElementById('treatmentMethodWarning');
            const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');

            fetch(`/api/Utils/GetAvailableTreatmentMethods/${treatmentRecordId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(methods => {
                    const choices = methods.map(method => ({
                        value: method.id,
                        label: method.name
                    }));

                    treatmentMethodChoices.setChoices(choices, 'value', 'label', true);

                    // Show warning and disable select if no methods available
                    if (methods.length === 0) {
                        treatmentMethodWarning.style.display = 'block';
                        treatmentMethodWarning.innerHTML = `
                            <div class="flex items-center">
                                <svg class="w-4 h-4 inline-block mr-1" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
                                    <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
                                </svg>
                                Không còn phương pháp điều trị phù hợp đối với Khoa bạn thuộc về.
                            </div>`;

                        // Disable treatment method select
                        treatmentMethodChoices.disable();
                        treatmentMethodSelect.classList.add('opacity-50', 'cursor-not-allowed');
                    } else {
                        treatmentMethodWarning.style.display = 'none';
                        // Enable treatment method select
                        treatmentMethodChoices.enable();
                        treatmentMethodSelect.classList.remove('opacity-50', 'cursor-not-allowed');
                    }
                })
                .catch(error => {
                    console.error('Error loading treatment methods:', error);
                    this.showError('Có lỗi xảy ra khi tải danh sách phương pháp điều trị');
                });
        },

        setupFlatpickr() {
            const self = this;

            // Initialize flatpickr for DateOfBirth and HealthInsuranceExpirationDate
            flatpickr(".flatpickr", {
                dateFormat: "d/m/Y",
                allowInput: true
            });

            // Get all required elements
            const elements = {
                treatmentStartDate: document.getElementById('treatmentRecordStartDate'),
                treatmentEndDate: document.getElementById('treatmentRecordEndDate'),
                assignmentStartDate: document.getElementById('assignmentStartDate'),
                assignmentEndDate: document.getElementById('assignmentEndDate'),
                assignmentDateWarning: document.getElementById('assignmentDateWarning')
            };

            // Check if all required elements exist
            for (const [key, element] of Object.entries(elements)) {
                if (!element) {
                    console.warn(`Required element ${key} is missing`);
                    return;
                }
            }

            // Function to check and update assignment date inputs state
            const updateAssignmentDateState = () => {
                const treatmentStartDate = elements.treatmentStartDate.value;
                const treatmentEndDate = elements.treatmentEndDate.value;
                const hasValidTreatmentDates = treatmentStartDate && treatmentEndDate;

                // Enable/disable assignment date inputs
                elements.assignmentStartDate.disabled = !hasValidTreatmentDates;
                elements.assignmentEndDate.disabled = !hasValidTreatmentDates;

                // Update visual state
                [elements.assignmentStartDate, elements.assignmentEndDate].forEach(input => {
                    if (hasValidTreatmentDates) {
                        input.classList.remove('opacity-50', 'cursor-not-allowed');
                    } else {
                        input.classList.add('opacity-50', 'cursor-not-allowed');
                        input.value = '';
                    }
                });

                // Show/hide warning message
                elements.assignmentDateWarning.style.display = hasValidTreatmentDates ? 'none' : 'block';

                // Auto-fill assignment start date with treatment start date ONLY if the assignment start date is empty
                if (treatmentStartDate && !elements.assignmentStartDate.value) {
                    elements.assignmentStartDate.value = treatmentStartDate;
                    // Trigger flatpickr update
                    elements.assignmentStartDate._flatpickr.setDate(treatmentStartDate);
                }
            };

            // Initial state check
            updateAssignmentDateState();

            try {
                // Setup flatpickr for treatment dates with validation
                const commonConfig = {
                    dateFormat: "d/m/Y",
                    allowInput: true,
                    onClose: updateAssignmentDateState
                };

                // Treatment start date
                flatpickr(elements.treatmentStartDate, {
                    ...commonConfig,
                    onChange: function (selectedDates, dateStr) {
                        updateAssignmentDateState();
                    }
                });

                // Treatment end date
                flatpickr(elements.treatmentEndDate, {
                    ...commonConfig,
                    onChange: function (selectedDates, dateStr) {
                        updateAssignmentDateState();
                    }
                });

                // Assignment dates
                flatpickr(elements.assignmentStartDate, {
                    ...commonConfig,
                    minDate: elements.treatmentStartDate.value || 'today'
                });

                flatpickr(elements.assignmentEndDate, {
                    ...commonConfig,
                    minDate: elements.assignmentStartDate.value || 'today'
                });

            } catch (error) {
                console.error('Error initializing flatpickr:', error);
                this.showError('Có lỗi xảy ra khi khởi tạo các trường ngày tháng');
            }
        },

        setupDropzone() {
            const self = this;

            // Add file input change listener
            const fileInput = document.getElementById('fileInput');
            fileInput?.addEventListener('change', (event) => {
                const file = event.target.files[0];
                if (file) {
                    if (self.dropzone.files.length > 0) {
                        self.dropzone.removeAllFiles();
                    }
                    self.dropzone.addFile(file);
                }
            });

            this.dropzone = new Dropzone('#imageDropzone', {
                url: '/Staff/Receptions/Edit',
                autoProcessQueue: false,
                maxFiles: 1,
                acceptedFiles: 'image/*',
                addRemoveLinks: false,
                dictDefaultMessage: 'Kéo thả hoặc nhấp để chọn ảnh',
                paramName: 'Patient.ImageFile',
                init: function () {
                    const existingImage = document.getElementById('existingImage').value;

                    if (existingImage && existingImage.trim() !== '') {
                        const mockFile = {
                            name: existingImage,
                            size: 12345,
                            accepted: true
                        };

                        this.emit('addedfile', mockFile);
                        this.emit('thumbnail', mockFile, `/Images/Patients/${existingImage}`);
                        this.files.push(mockFile);

                        // Hide the default message when there's an existing image
                        const messageElement = this.element.querySelector('.dz-message');
                        if (messageElement) {
                            messageElement.style.display = 'none';
                        }
                    }

                    this.on('addedfile', (file) => {
                        if (this.files.length > 1) {
                            this.removeFile(this.files[0]);
                        }
                        // Show preview container and hide message
                        const messageElement = this.element.querySelector('.dz-message');
                        if (messageElement) messageElement.style.display = 'none';
                    });

                    this.on('removedfile', () => {
                        // Show message when no files
                        if (this.files.length === 0) {
                            const messageElement = this.element.querySelector('.dz-message');
                            if (messageElement) messageElement.style.display = 'block';
                        }
                    });

                    this.on('sending', (file, xhr, formData) => {
                        self.appendFormData(formData);
                    });

                    this.on('success', (file, response) => {
                        self.handleResponse(response);
                    });

                    this.on('error', (file, errorMessage) => {
                        console.error('Dropzone error:', errorMessage);
                        notyf.error("Có lỗi xảy ra: " + errorMessage);
                    });
                }
            });
        },

        filterRooms(treatmentMethodId) {
            const roomSelect = document.getElementById('treatmentRecordDetailRoom');
            const treatmentMethodWarning = document.getElementById('treatmentMethodWarning');
            if (!roomSelect || !this.roomChoices) return;

            if (!treatmentMethodId) {
                this.roomChoices.clearStore();
                this.roomChoices.disable();
                treatmentMethodWarning.style.display = 'block';
                treatmentMethodWarning.innerHTML = `
                    <div class="flex items-center">
                        <svg class="w-4 h-4 inline-block mr-1" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
                            <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
                        </svg>
                        Vui lòng chọn phương pháp điều trị trước khi chọn phòng!
                    </div>`;
                return;
            }

            // Enable room select and show loading state
            this.roomChoices.enable();
            this.roomChoices.clearStore();
            this.roomChoices.setChoices([{ value: '', label: 'Đang tải...', disabled: true }], 'value', 'label', true);

            // Hide warning when treatment method is selected
            treatmentMethodWarning.style.display = 'none';

            // Fetch rooms based on treatment method
            fetch(`/api/Utils/GetRoomsByTreatmentMethod/${treatmentMethodId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(rooms => {

                    if (!rooms || rooms.length === 0) {
                        this.roomChoices.clearStore();
                        this.roomChoices.disable();
                        treatmentMethodWarning.style.display = 'block';
                        treatmentMethodWarning.innerHTML = `
                            <div class="flex items-center">
                                <svg class="w-4 h-4 inline-block mr-1" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
                                    <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
                                </svg>
                                Không còn phòng phù hợp với phương pháp điều trị này
                            </div>`;
                        return;
                    }

                    // Update room choices
                    const roomChoices = rooms.map(room => ({
                        value: room.id.toString(),
                        label: room.name,
                        selected: room.id === this.currentRoomId,
                    }));

                    this.roomChoices.setChoices(roomChoices, 'value', 'label', true);
                })
                .catch(error => {
                    console.error('Error fetching rooms:', error);
                    this.showError('Có lỗi xảy ra khi tải danh sách phòng');
                    this.roomChoices.clearStore();
                    this.roomChoices.disable();
                });
        },

        showWarning(message) {
            console.log(message);
        },

        showError(message) {
            console.log(message);
        },

        appendFormData(formData) {
            // Treatment record information
            formData.append('TreatmentRecord.Id', document.getElementById('treatmentRecordId')?.value || '');
            formData.append('TreatmentRecord.PatientId', document.getElementById('patientId')?.value || '');
            formData.append('TreatmentRecord.Code', document.querySelector('[name="TreatmentRecord.Code"]')?.value || document.getElementById('treatmentRecordCode')?.value || '');
            formData.append('TreatmentRecord.Diagnosis', document.querySelector('[name="TreatmentRecord.Diagnosis"]')?.value || '');
            formData.append('TreatmentRecord.StartDate', document.querySelector('[name="TreatmentRecord.StartDate"]')?.value || '');
            formData.append('TreatmentRecord.EndDate', document.querySelector('[name="TreatmentRecord.EndDate"]')?.value || '');
            formData.append('TreatmentRecord.Note', document.querySelector('[name="TreatmentRecord.Note"]')?.value || '');

            // Patient information
            formData.append('Patient.Code', document.querySelector('[name="Patient.Code"]')?.value || document.getElementById('patientCode')?.value || '');
            formData.append('Patient.Name', document.querySelector('[name="Patient.Name"]')?.value || '');
            formData.append('Patient.DateOfBirth', document.querySelector('[name="Patient.DateOfBirth"]')?.value || '');
            formData.append('Patient.Gender', document.querySelector('[name="Patient.Gender"]')?.value || '');
            formData.append('Patient.IdentityNumber', document.querySelector('[name="Patient.IdentityNumber"]')?.value || '');
            formData.append('Patient.PhoneNumber', document.querySelector('[name="Patient.PhoneNumber"]')?.value || '');
            formData.append('Patient.Address', document.querySelector('[name="Patient.Address"]')?.value || '');
            formData.append('Patient.EmailAddress', document.querySelector('[name="Patient.EmailAddress"]')?.value || '');

            // Health insurance information
            const healthInsuranceCheckbox = document.querySelector('[name="Patient.HasHealthInsurance"]');
            const hasHealthInsurance = healthInsuranceCheckbox ? healthInsuranceCheckbox.checked : false;
            formData.append('Patient.HasHealthInsurance', hasHealthInsurance);

            // Health insurance information (if applicable)
            if (hasHealthInsurance) {
                formData.append('Patient.HealthInsuranceCode', document.querySelector('[name="Patient.HealthInsuranceCode"]')?.value || '');
                formData.append('Patient.HealthInsuranceNumber', document.querySelector('[name="Patient.HealthInsuranceNumber"]')?.value || '');
                formData.append('Patient.HealthInsuranceExpiryDate', document.querySelector('[name="Patient.HealthInsuranceExpiryDate"]')?.value || '');
                formData.append('Patient.HealthInsurancePlaceOfRegistration', document.querySelector('[name="Patient.HealthInsurancePlaceOfRegistration"]')?.value || '');
                formData.append('Patient.HealthInsuranceIsRightRoute', document.querySelector('[name="Patient.HealthInsuranceIsRightRoute"]')?.checked || false);
            }

            // New treatment record detail (if form is shown)
            const showAddTreatmentForm = document.querySelector("[x-show='showAddTreatmentForm']");
            if (showAddTreatmentForm && window.getComputedStyle(showAddTreatmentForm).display !== 'none') {
                // Get treatment method select element
                const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');
                const treatmentMethodId = treatmentMethodSelect ? treatmentMethodSelect.value : '';
                const roomId = document.querySelector('[name="NewTreatmentRecordDetail.RoomId"]')?.value;
                const note = document.querySelector('[name="NewTreatmentRecordDetail.Note"]')?.value;

                formData.append('NewTreatmentRecordDetail.Code', document.querySelector('[name="NewTreatmentRecordDetail.Code"]')?.value || this.generateCode());
                formData.append('NewTreatmentRecordDetail.TreatmentMethodId', treatmentMethodId);
                formData.append('NewTreatmentRecordDetail.RoomId', roomId || '');
                formData.append('NewTreatmentRecordDetail.Note', note || '');
            }

            // New assignment (if form is shown)
            const showAddAssignmentForm = document.querySelector("[x-show='showAddAssignmentForm']");
            if (showAddAssignmentForm && window.getComputedStyle(showAddAssignmentForm).display !== 'none') {
                formData.append('NewAssignment.Code', document.querySelector('[name="NewAssignment.Code"]')?.value || this.generateCode());
                formData.append('NewAssignment.StartDate', document.querySelector('[name="NewAssignment.StartDate"]')?.value || '');
                formData.append('NewAssignment.EndDate', document.querySelector('[name="NewAssignment.EndDate"]')?.value || '');
                formData.append('NewAssignment.Note', document.querySelector('[name="NewAssignment.Note"]')?.value || '');
            }

            // Regulations
            this.regulations.forEach((reg, index) => {
                // Bỏ qua dòng chưa chọn quy định hoặc chưa chọn ngày thực hiện
                if (!reg.RegulationId || !reg.ExecutionDate) return;
                formData.append(`Regulations[${index}].Code`, reg.Code || '');
                formData.append(`Regulations[${index}].RegulationId`, reg.RegulationId || '');
                formData.append(`Regulations[${index}].ExecutionDate`, reg.ExecutionDate || '');
                formData.append(`Regulations[${index}].Note`, reg.Note || '');
            });

            // Current employee ID
            const currentEmployeeId = document.getElementById('currentEmployeeId')?.value;
            if (currentEmployeeId) {
                formData.append('CurrentEmployeeId', currentEmployeeId);
            }

            // Append antiforgery token
            const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (antiforgeryToken) {
                formData.append('__RequestVerificationToken', antiforgeryToken);
            }
        },

        handleResponse(response) {
            if (response.success) {
                notyf.success(response.message);
                setTimeout(() => {
                    window.location.href = '/Staff/TreatmentRecords';
                }, 1500);
            } else {
                this.overlay.style.display = 'none';
                notyf.error(response.message);
            }
        },

        setupValidation() {
            $.validator.addMethod("customPattern", function (value, element, regex) {
                return this.optional(element) || regex.test(value);
            }, "Giá trị không hợp lệ.");

            $.validator.addMethod("dateFormat", function (value, element) {
                return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
            }, "Vui lòng nhập ngày hợp lệ (dd/mm/yyyy).");

            $.validator.addMethod("notPastDate", function (value, element) {
                if (!value) return true;

                var dateParts = value.split("/");
                var inputDate = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
                var today = new Date();
                today.setHours(0, 0, 0, 0);

                return inputDate >= today;
            }, "Ngày bắt đầu không được là ngày trong quá khứ");

            $.validator.addMethod("endDateAfterStartDate", function (value, element) {
                var startDate = $("input[name='TreatmentRecord.StartDate']").val();
                if (!startDate || !value) return true;

                var startParts = startDate.split("/");
                var endParts = value.split("/");

                var start = new Date(startParts[2], startParts[1] - 1, startParts[0]);
                var end = new Date(endParts[2], endParts[1] - 1, endParts[0]);

                return end >= start;
            }, "Ngày kết thúc phải sau ngày bắt đầu");

            $.validator.addMethod("phone", function (value, element) {
                return this.optional(element) || /^\+?\d{10,12}$/.test(value);
            }, "Số điện thoại không hợp lệ.");

            $.validator.addMethod("notExpired", function (value, element) {
                if (!value) return true; // Skip validation if no date entered

                const parts = value.split('/');
                const expiryDate = new Date(parts[2], parts[1] - 1, parts[0]);
                const today = new Date();
                today.setHours(0, 0, 0, 0);

                return expiryDate >= today;
            }, "Thẻ BHYT đã hết hạn");

            // Add validation rules for new assignment form
            $.validator.addMethod("assignmentStartDateValid", function (value, element) {
                if (!$("[x-show='showAddAssignmentForm']").is(":visible")) return true;

                if (!value) return false;

                var treatmentStartDate = $("input[name='TreatmentRecord.StartDate']").val();
                var treatmentEndDate = $("input[name='TreatmentRecord.EndDate']").val();

                if (!treatmentStartDate || !treatmentEndDate) return false;

                var dateParts = value.split("/");
                var assignmentStartDate = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);

                var treatmentStartParts = treatmentStartDate.split("/");
                var treatmentEndParts = treatmentEndDate.split("/");

                var treatmentStart = new Date(treatmentStartParts[2], treatmentStartParts[1] - 1, treatmentStartParts[0]);
                var treatmentEnd = new Date(treatmentEndParts[2], treatmentEndParts[1] - 1, treatmentEndParts[0]);

                return assignmentStartDate >= treatmentStart && assignmentStartDate <= treatmentEnd;
            }, "Ngày bắt đầu phân công phải nằm trong khoảng thời gian điều trị");

            $.validator.addMethod("assignmentEndDateValid", function (value, element) {
                if (!$("[x-show='showAddAssignmentForm']").is(":visible")) return true;

                if (!value) return false;

                var treatmentStartDate = $("input[name='TreatmentRecord.StartDate']").val();
                var treatmentEndDate = $("input[name='TreatmentRecord.EndDate']").val();
                var assignmentStartDate = $("input[name='NewAssignment.StartDate']").val();

                if (!treatmentStartDate || !treatmentEndDate || !assignmentStartDate) return false;

                var dateParts = value.split("/");
                var assignmentEndDate = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);

                var treatmentStartParts = treatmentStartDate.split("/");
                var treatmentEndParts = treatmentEndDate.split("/");
                var assignmentStartParts = assignmentStartDate.split("/");

                var treatmentStart = new Date(treatmentStartParts[2], treatmentStartParts[1] - 1, treatmentStartParts[0]);
                var treatmentEnd = new Date(treatmentEndParts[2], treatmentEndParts[1] - 1, treatmentEndParts[0]);
                var assignmentStart = new Date(assignmentStartParts[2], assignmentStartParts[1] - 1, assignmentStartParts[0]);

                return assignmentEndDate >= assignmentStart && assignmentEndDate <= treatmentEnd;
            }, "Ngày kết thúc phân công phải sau ngày bắt đầu phân công và nằm trong khoảng thời gian điều trị");

            $("#receptionForm").validate({
                ignore: [],
                rules: {
                    "Patient.Name": {
                        required: true,
                        minlength: 6,
                        maxlength: 20,
                        customPattern: /^[A-Za-zÀ-ỹ][A-Za-zÀ-ỹ0-9 ]*$/
                    },
                    "Patient.Gender": {
                        required: true
                    },
                    "Patient.DateOfBirth": {
                        required: true,
                        dateFormat: true
                    },
                    "Patient.IdentityNumber": {
                        required: function () {
                            var dob = $("input[name='Patient.DateOfBirth']").val();
                            if (!dob) return false;
                            var dateParts = dob.split("/");
                            var dobDate = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
                            var today = new Date();
                            var age = today.getFullYear() - dobDate.getFullYear();
                            var monthDiff = today.getMonth() - dobDate.getMonth();
                            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dobDate.getDate())) {
                                age--;
                            }
                            return age >= 14;
                        },
                        minlength: 9,
                        maxlength: 12,
                        customPattern: /^[0-9]*$/,
                        remote: {
                            url: "/api/validation/patient/check",
                            type: "GET",
                            data: {
                                type: "identitynumber",
                                entityType: "patient",
                                value: function () { return $("#IdentityNumber").val(); },
                                id: function () { return $("#patientId").val(); }
                            },
                            dataFilter: function (data) {
                                try {
                                    const response = JSON.parse(data);
                                    if (response.success) {
                                        return response.isUnique === true;
                                    } else {
                                        notyf.error(response.message || "Lỗi khi kiểm tra CCCD.");
                                        return false;
                                    }
                                } catch (e) {
                                    notyf.error("Lỗi kết nối server.");
                                    return false;
                                }
                            }
                        }
                    },
                    "Patient.Address": {
                        required: true
                    },
                    "Patient.EmailAddress": {
                        email: true
                    },
                    "Patient.PhoneNumber": {
                        required: true,
                        minlength: 11,
                        maxlength: 11,
                        phone: true
                    },
                    "TreatmentRecord.StartDate": {
                        required: true,
                        dateFormat: true
                    },
                    "TreatmentRecord.EndDate": {
                        required: true,
                        dateFormat: true,
                        endDateAfterStartDate: true
                    },
                    "TreatmentRecord.Diagnosis": {
                        required: true
                    },
                    "Patient.HealthInsuranceNumber": {
                        required: () => $('#HasHealthInsurance').is(':checked'),
                        minlength: 15,
                        maxlength: 15,
                        customPattern: /^[0-9A-Z]*$/,
                        remote: {
                            url: "/api/validation/healthinsurance/check",
                            type: "GET",
                            data: {
                                entityType: "healthinsurance",
                                type: "numberhealthinsurance",
                                value: function () { return $("#HealthInsuranceNumber").val(); },
                                id: function () { return $("#healthInsuranceId").val(); }
                            },
                            dataFilter: function (data) {
                                try {
                                    const response = JSON.parse(data);
                                    if (response.success) {
                                        return response.isUnique === true;
                                    } else {
                                        notyf.error(response.message || "Lỗi khi kiểm tra số BHYT.");
                                        return false;
                                    }
                                } catch (e) {
                                    notyf.error("Lỗi kết nối server.");
                                    return false;
                                }
                            }
                        }
                    },
                    "Patient.HealthInsuranceExpiryDate": {
                        required: () => $('#HasHealthInsurance').is(':checked'),
                        dateFormat: true,
                        notExpired: true
                    },
                    "Patient.HealthInsurancePlaceOfRegistration": {
                        required: () => $('#HasHealthInsurance').is(':checked')
                    },
                    "NewAssignment.StartDate": {
                        required: function () {
                            return $("[x-show='showAddAssignmentForm']").is(":visible");
                        },
                        dateFormat: true,
                        assignmentStartDateValid: true
                    },
                    "NewAssignment.EndDate": {
                        required: function () {
                            return $("[x-show='showAddAssignmentForm']").is(":visible");
                        },
                        dateFormat: true,
                        assignmentEndDateValid: true
                    }
                },
                messages: {
                    "Patient.Name": {
                        required: "Tên bệnh nhân không được bỏ trống",
                        minlength: "Tên phải có ít nhất 6 ký tự",
                        maxlength: "Tên không được vượt quá 20 ký tự",
                        customPattern: "Tên phải bắt đầu bằng chữ cái và chỉ chứa chữ cái, số hoặc khoảng trắng"
                    },
                    "Patient.Gender": {
                        required: "Giới tính không được bỏ trống"
                    },
                    "Patient.DateOfBirth": {
                        required: "Ngày sinh không được bỏ trống",
                        dateFormat: "Ngày sinh không hợp lệ"
                    },
                    "Patient.IdentityNumber": {
                        required: "Căn cước công dân không được bỏ trống",
                        minlength: "Căn cước công dân có ít nhất 9 số",
                        maxlength: "Căn cước công dân không vượt quá 12 số",
                        customPattern: "Căn cước công dân không hợp lệ",
                        remote: "Căn cước công dân đã được đăng ký trên hệ thống"
                    },
                    "Patient.Address": {
                        required: "Địa chỉ không được bỏ trống"
                    },
                    "Patient.EmailAddress": {
                        email: "Email không hợp lệ"
                    },
                    "Patient.PhoneNumber": {
                        required: "Số điện thoại không được bỏ trống",
                        minlength: "Số điện thoại phải có ít nhất 11 ký tự",
                        maxlength: "Số điện thoại không được vượt quá 11 ký tự",
                        phone: "Số điện thoại không hợp lệ"
                    },
                    "TreatmentRecord.StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống",
                        dateFormat: "Ngày bắt đầu không hợp lệ"
                    },
                    "TreatmentRecord.EndDate": {
                        required: "Ngày kết thúc không được bỏ trống",
                        dateFormat: "Ngày kết thúc không hợp lệ",
                        endDateAfterStartDate: "Ngày kết thúc phải sau ngày bắt đầu"
                    },
                    "TreatmentRecord.Diagnosis": {
                        required: "Chẩn đoán không được bỏ trống"
                    },
                    "Patient.HealthInsuranceNumber": {
                        required: "Số BHYT không được bỏ trống",
                        minlength: "Số BHYT phải có 15 ký tự",
                        maxlength: "Số BHYT phải có 15 ký tự",
                        customPattern: "Số BHYT chỉ được nhập số và chữ in hoa",
                        remote: "Số BHYT đã được đăng ký trên hệ thống"
                    },
                    "Patient.HealthInsuranceExpiryDate": {
                        required: "Ngày hết hạn không được bỏ trống",
                        dateFormat: "Ngày hết hạn không hợp lệ",
                        notExpired: "Thẻ BHYT đã hết hạn"
                    },
                    "Patient.HealthInsurancePlaceOfRegistration": {
                        required: "Nơi đăng ký không được bỏ trống"
                    },
                    "NewAssignment.StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống",
                        dateFormat: "Ngày bắt đầu không hợp lệ"
                    },
                    "NewAssignment.EndDate": {
                        required: "Ngày kết thúc không được bỏ trống",
                        dateFormat: "Ngày kết thúc không hợp lệ"
                    }
                },
                errorElement: "div",
                errorClass: "text-danger",
                highlight: function (element) {
                    if ($(element).is('select')) {
                        const choicesContainer = $(element).closest('.select-wrapper').find('.choices__inner');
                        choicesContainer.addClass("border-red-500");
                    } else {
                        $(element).addClass("border-red-500");
                    }
                },
                unhighlight: function (element) {
                    if ($(element).is('select')) {
                        const choicesContainer = $(element).closest('.select-wrapper').find('.choices__inner');
                        choicesContainer.removeClass("border-red-500");
                    } else {
                        $(element).removeClass("border-red-500");
                    }
                },
                errorPlacement: function (error, element) {
                    if (element.is('select')) {
                        const wrapper = element.closest('.select-wrapper');
                        if (wrapper.length) {
                            error.insertAfter(wrapper);
                        } else {
                            error.insertAfter(element);
                        }
                    } else {
                        error.insertAfter(element);
                    }
                },
                onfocusout: function (element) {
                    if ($(element).val() === '' || $(element).val().length > 0) {
                        this.element(element);
                    }
                },
                onkeyup: false
            });

            $('select.form-input').on('change', function () {
                $(this).valid();
            });

            // Add submit handler for the form
            const self = this;
            $("#receptionForm").on('submit', function (e) {
                e.preventDefault();
                self.update();
            });
        },

        update() {
            const currentId = (window.currentEmployeeId || '').toLowerCase();
            const hasTreatment = (window.treatmentRecordDetailsData || []).some(d => (d.employeeId || '').toLowerCase() === currentId);
            const hasAssignment = (window.assignmentsData || []).some(a => (a.employeeId || '').toLowerCase() === currentId);

            // Kiểm tra trạng thái form "Thêm mới"
            const isAddingTreatment = this.showAddTreatmentForm;
            const isAddingAssignment = this.showAddAssignmentForm;

            // Bác sĩ mới: chưa có bản ghi nào
            const isNewDoctor = !hasTreatment && !hasAssignment;

            if (isNewDoctor) {
                // Nếu chỉ điền 1 trong 2
                if (isAddingTreatment && !isAddingAssignment) {
                    notyf.error("Bạn cần bổ sung thông tin phân công");
                    return;
                }
                if (!isAddingTreatment && isAddingAssignment) {
                    notyf.error("Bạn cần bổ sung thông tin điều trị");
                    return;
                }
            }

            const form = document.getElementById('receptionForm');
            if (!form) {
                notyf.error("Không tìm thấy form. Vui lòng tải lại trang.");
                return;
            }

            if (!$(form).valid()) {
                notyf.error("Vui lòng kiểm tra lại thông tin đã nhập.");
                return;
            }

            try {
                if (!this.overlay) {
                    notyf.error("Không tìm thấy overlay loading. Vui lòng tải lại trang.");
                    return;
                }
                this.overlay.style.display = 'flex';

                // Kiểm tra dropzone
                if (!this.dropzone) {
                    notyf.error("Không tìm thấy dropzone. Vui lòng tải lại trang.");
                    this.overlay.style.display = 'none';
                    return;
                }

                if (this.dropzone.files.length > 0 && this.dropzone.getQueuedFiles().length > 0) {
                    this.dropzone.processQueue();
                } else {
                    const formData = new FormData();

                    // Append other form data
                    this.appendFormData(formData);

                    if (!validateRegulations()) {
                        notyf.error("Vui lòng kiểm tra thông tin nhập.");
                        this.overlay.style.display = 'none';
                        return;
                    }

                    fetch('/Staff/Receptions/Edit', {
                        method: 'POST',
                        body: formData
                    })
                        .then(response => {
                            if (!response.ok) {
                                return response.text().then(text => {
                                    throw new Error(`HTTP error! status: ${response.status}, message: ${text}`);
                                });
                            }
                            return response.json();
                        })
                        .then(response => {
                            this.handleResponse(response);
                        })
                        .catch(error => {
                            this.overlay.style.display = 'none';
                            console.error("Chi tiết lỗi:", error);

                            // Hiển thị thông báo lỗi chi tiết hơn
                            if (error.name === 'TypeError') {
                                notyf.error(`Lỗi dữ liệu: ${error.message}. Vui lòng kiểm tra lại thông tin nhập.`);
                            } else if (error.message.includes('HTTP error')) {
                                notyf.error(`Lỗi kết nối server: ${error.message}. Vui lòng thử lại sau.`);
                            } else {
                                notyf.error(`Lỗi: ${error.message}. Vui lòng liên hệ admin.`);
                            }
                        });
                }
            } catch (error) {
                if (this.overlay) {
                    this.overlay.style.display = 'none';
                }
                console.error("Chi tiết lỗi:", {
                    name: error.name,
                    message: error.message,
                    stack: error.stack
                });

                // Hiển thị thông báo lỗi chi tiết
                if (error.name === 'TypeError') {
                    notyf.error(`Lỗi dữ liệu: ${error.message}. Vui lòng kiểm tra lại thông tin nhập.`);
                } else {
                    notyf.error(`Lỗi xử lý: ${error.message}. Vui lòng liên hệ admin.`);
                }
            }
        },

        /**
         * Generate a random code
         */
        generateCode() {
            const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
            let result = '';
            for (let i = 0; i < 10; i++) {
                result += chars.charAt(Math.floor(Math.random() * chars.length));
            }
            return result;
        },

        addRegulation() {
            if (this.regulations.length >= 5) {
                notyf.error('Không thể thêm quá 5 quy định cho một phiếu điều trị');
                return;
            }
            const availableRegs = this.getAvailableRegulationsForAdd();
            if (availableRegs.length === 0) {
                notyf.error('Không có quy định nào phù hợp trong khoảng thời gian điều trị.');
                return;
            }
            const code = this.generateCode();
            this.regulations.push({
                Code: code,
                RegulationId: '',
                ExecutionDate: '',
                Note: '',
                isOld: false
            });
            this.$nextTick(() => {
                this.initFlatpickrForAll();
            });
        },
        removeRegulation(index) {
            this.regulations.splice(index, 1);
        },
        getAvailableRegulationsForAdd() {
            // Lấy ngày bắt đầu/kết thúc điều trị
            const treatmentStart = document.getElementById('treatmentRecordStartDate')?.value;
            const treatmentEnd = document.getElementById('treatmentRecordEndDate')?.value;
            if (!treatmentStart || !treatmentEnd) return [];
            const startDate = this.parseVNDate(treatmentStart);
            const endDate = this.parseVNDate(treatmentEnd);
            // Lấy các RegulationId đã chọn
            const selectedIds = this.regulations.map(r => r.RegulationId).filter(id => id);
            // Lọc quy định hợp lệ
            return this.allRegulations.filter(r => {
                if (selectedIds.includes(r.id.toString())) return false;
                const regStart = this.parseVNDate(r.effectiveStartDate);
                const regEnd = this.parseVNDate(r.effectiveEndDate);
                // Quy định phải giao với khoảng điều trị
                return regStart <= endDate && regEnd >= startDate;
            });
        },
        getAvailableRegulations(currentIndex) {
            const treatmentStart = document.getElementById('treatmentRecordStartDate')?.value;
            const treatmentEnd = document.getElementById('treatmentRecordEndDate')?.value;
            if (!treatmentStart || !treatmentEnd) return [];
            const startDate = this.parseVNDate(treatmentStart);
            const endDate = this.parseVNDate(treatmentEnd);
            const selectedIds = this.regulations.map((r, idx) => idx !== currentIndex ? r.RegulationId : null).filter(id => id);
            let available = this.allRegulations.filter(r => {
                if (selectedIds.includes(r.id.toString())) return false;
                const regStart = this.parseVNDate(r.effectiveStartDate);
                const regEnd = this.parseVNDate(r.effectiveEndDate);
                return regStart <= endDate && regEnd >= startDate;
            });
            // Nếu regulation hiện tại đã chọn nhưng không có trong allRegulations, thêm vào option
            const currentReg = this.regulations[currentIndex];
            if (currentReg && currentReg.RegulationId && currentReg.RegulationName) {
                const exists = available.some(r => r.id.toString() === currentReg.RegulationId.toString());
                if (!exists) {
                    available = [
                        {
                            id: currentReg.RegulationId,
                            name: currentReg.RegulationName,
                            effectiveStartDate: '',
                            effectiveEndDate: ''
                        },
                        ...available
                    ];
                }
            }
            return available;
        },
        onRegulationChange(index, value) {
            this.regulations[index].RegulationId = value;
            this.regulations[index].ExecutionDate = '';
            this.$nextTick(() => {
                this.initFlatpickrForAll();
            });
        },
        initFlatpickrForAll() {
            this.regulations.forEach((reg, idx) => {
                const dateInput = document.getElementById('executionDate-' + idx);
                if (!dateInput) return;
                if (dateInput._flatpickr) {
                    dateInput._flatpickr.destroy();
                }
                if (!reg.RegulationId) {
                    dateInput.value = '';
                    dateInput.disabled = true;
                    dateInput.classList.add('opacity-50', 'cursor-not-allowed');
                    dateInput.style.cursor = 'not-allowed';
                    return;
                }
                // Lấy ngày hiệu lực của quy định
                const regulation = this.allRegulations.find(r => r.id.toString() === reg.RegulationId);
                if (!regulation) return;
                // Lấy ngày bắt đầu và kết thúc điều trị
                const treatmentStart = document.getElementById('treatmentRecordStartDate')?.value;
                const treatmentEnd = document.getElementById('treatmentRecordEndDate')?.value;
                const minDate = this.maxDateVN(regulation.effectiveStartDate, treatmentStart);
                const maxDate = this.minDateVN(regulation.effectiveEndDate, treatmentEnd);
                dateInput.disabled = false;
                dateInput.classList.remove('opacity-50', 'cursor-not-allowed');
                dateInput.style.cursor = '';
                flatpickr(dateInput, {
                    dateFormat: 'd/m/Y',
                    minDate: minDate,
                    maxDate: maxDate,
                    allowInput: true,
                    onChange: (selectedDates, dateStr) => {
                        this.regulations[idx].ExecutionDate = dateStr;
                    }
                });
            });
        },
        maxDateVN(date1, date2) {
            const d1 = this.parseVNDate(date1);
            const d2 = this.parseVNDate(date2);
            if (!d1) return date2;
            if (!d2) return date1;
            return d1 > d2 ? date1 : date2;
        },
        minDateVN(date1, date2) {
            const d1 = this.parseVNDate(date1);
            const d2 = this.parseVNDate(date2);
            if (!d1) return date2;
            if (!d2) return date1;
            return d1 < d2 ? date1 : date2;
        },
        parseVNDate(str) {
            if (!str) return null;
            const [day, month, year] = str.split('/').map(Number);
            return new Date(year, month - 1, day);
        },
        setupChoices() {
            // Khởi tạo choices cho tất cả select có class 'choices'
            const selectElements = document.querySelectorAll('.choices');
            selectElements.forEach(select => {
                // Skip if already initialized or is treatment method or room select
                if (select.id === 'treatmentRecordDetailRoom' ||
                    select.id === 'treatmentRecordDetailTreatmentMethod' ||
                    select.choices) return;

                new Choices(select, {
                    searchEnabled: true,
                    searchPlaceholderValue: 'Tìm kiếm...',
                    removeItemButton: true,
                    noResultsText: 'Không tìm thấy kết quả',
                    noChoicesText: 'Không có lựa chọn nào',
                    itemSelectText: ''
                });
            });

            this.initRoomChoices();
            this.initTreatmentMethodChoices();
        },
        initRoomChoices() {
            const roomSelect = document.getElementById('treatmentRecordDetailRoom');
            if (!roomSelect || roomSelect.choices) return; // Skip if already initialized

            if (this.roomChoices) {
                this.roomChoices.destroy();
            }

            this.roomChoices = new Choices(roomSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm phòng...',
                removeItemButton: true,
                noResultsText: 'Không tìm thấy phòng',
                noChoicesText: 'Không có phòng nào',
                itemSelectText: '',
                placeholder: true,
                placeholderValue: 'Chọn phòng'
            });

            roomSelect.choices = this.roomChoices; // Mark as initialized
            this.roomChoices.disable(); // Initially disable room select
        },
        initTreatmentMethodChoices() {
            const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');
            if (!treatmentMethodSelect || treatmentMethodSelect.choices) return;

            const treatmentMethodChoices = new Choices(treatmentMethodSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm phương pháp điều trị...',
                removeItemButton: false,
                noResultsText: 'Không tìm thấy kết quả',
                noChoicesText: 'Không có phương pháp điều trị nào',
                itemSelectText: '',
                allowHTML: true
            });

            // Disable the select since it's pre-filled with employee's treatment method
            treatmentMethodChoices.disable();
            treatmentMethodSelect.classList.add('opacity-75');
        },
        goBack() {
            window.history.back();
        },
        handleAddTreatment() {
            const currentId = (window.currentEmployeeId || '').toLowerCase();

            const exists = (window.treatmentRecordDetailsData || []).some(d =>
                (d.employeeId || '').toLowerCase() === currentId
            );

            if (exists) {
                notyf.error('Bạn đã tham gia vào phiếu điều trị này rồi');
                return;
            }
            this.showAddTreatmentForm = true;
        },
        handleAddAssignment() {
            const currentId = (window.currentEmployeeId || '').toLowerCase();

            const exists = (window.assignmentsData || []).some(a =>
                (a.employeeId || '').toLowerCase() === currentId
            );

            if (exists) {
                notyf.error('Bạn đã tham gia vào phiếu điều trị này rồi');
                return;
            }
            this.showAddAssignmentForm = true;
        }
    }));
});

// Make Alpine data instance globally accessible
let editData;
document.addEventListener('alpine:initialized', () => {
    editData = Alpine.store('editData');
});

// Khởi tạo các nút xem ghi chú khi trang load
$(document).ready(function () {
    // Tìm tất cả các nút xem ghi chú và thiết lập event handler
    $('button[data-note]').each(function () {
        const button = $(this);
        const code = button.data('code');
        const note = button.data('note');
        setupNoteButton(button, code, note);
    });
});

function editTreatmentDetail(code) {
    // Đảm bảo có thông tin bác sĩ trước
    ensureCurrentDoctor().then(() => {
        // Lấy thông tin chi tiết điều trị từ API
        $.get(`/api/Utils/GetTreatmentDetail/${code}`, function (detail) {
            console.log('currentDoctor:', currentDoctor);
            console.log('detail:', detail);

            if (!detail) {
                notyf.error('Không tìm thấy thông tin điều trị');
                return;
            }

            if (!currentDoctor || (detail.employeeId || '').toLowerCase() !== (currentDoctor.id || '').toLowerCase()) {
                notyf.error('Bạn không có quyền chỉnh sửa bản ghi của bác sĩ khác');
                return;
            }

            if (!detail.treatmentMethodId) {
                notyf.error('Không tìm thấy thông tin phương pháp điều trị');
                return;
            }

            $.confirm({
                title: 'Chỉnh sửa thông tin điều trị',
                content: `
                    <form id="treatmentEditForm">
                        <div class="flex gap-4 mb-4">
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Mã chi tiết</label>
                                <input type="text" class="form-input w-full" value="${detail.code}" readonly>
                            </div>
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Phương pháp điều trị</label>
                                <select class="form-select w-full" id="treatmentRecordDetailTreatmentMethod">
                                    <option value="${detail.treatmentMethodId}">${detail.treatmentMethodName}</option>
                                </select>
                            </div>
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Phòng điều trị</label>
                                <select class="form-select w-full" id="treatmentRecordDetailRoom">
                                    <option value="${detail.roomId}">${detail.roomName}</option>
                                </select>
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="font-semibold mb-2">Ghi chú</label>
                            <textarea class="form-textarea w-full" rows="3">${detail.note || 'Không có ghi chú'}</textarea>
                        </div>
                    </form>`,
                type: 'blue',
                typeAnimated: true,
                theme: 'modern',
                columnClass: 'medium',
                boxWidth: '1000px',
                useBootstrap: false,
                buttons: {
                    save: {
                        text: 'Lưu',
                        btnClass: 'btn-primary',
                        action: function () {
                            const roomId = this.$content.find('#treatmentRecordDetailRoom').val();
                            const note = this.$content.find('textarea').val();

                            if (!roomId) {
                                notyf.warning('Vui lòng chọn phòng điều trị');
                                return false;
                            }

                            // Gửi request cập nhật
                            $.ajax({
                                url: '/api/Utils/UpdateTreatmentDetail',
                                type: 'POST',
                                contentType: 'application/json',
                                data: JSON.stringify({
                                    code: code,
                                    roomId: roomId,
                                    note: note || ''
                                }),
                                success: function (response) {
                                    // Cập nhật thông tin trên bảng
                                    const row = $(`tr:has(td:contains('${code}'))`);
                                    row.find('td:eq(2)').text(response.roomName);
                                    row.find('td:eq(3)').text(response.treatmentMethodName);

                                    // Cập nhật nút xem ghi chú
                                    const noteButton = row.find('td:eq(4)').find('button');
                                    setupNoteButton(noteButton, response.code, response.note);

                                    // Hiển thị thông báo thành công
                                    notyf.success('Đã cập nhật thông tin điều trị');
                                },
                                error: function (xhr) {
                                    notyf.error(xhr.responseText || 'Có lỗi xảy ra khi cập nhật thông tin');
                                }
                            });
                        }
                    },
                    close: {
                        text: 'Đóng',
                        btnClass: 'btn-default'
                    }
                },
                onContentReady: function () {
                    const roomSelect = this.$content.find('#treatmentRecordDetailRoom');

                    // Xóa hết option cũ trước khi thêm mới
                    roomSelect.empty();

                    // Load danh sách phòng theo phương pháp điều trị
                    $.get(`/api/Utils/GetRoomsByTreatmentMethod/${detail.treatmentMethodId}`, function (rooms) {
                        if (!rooms || rooms.length === 0) {
                            notyf.error('Không tìm thấy phòng phù hợp với phương pháp điều trị này');
                            return;
                        }

                        rooms.forEach(room => {
                            const option = new Option(room.name, room.id, false, room.id === detail.roomId);
                            roomSelect.append(option);
                        });
                    }).fail(function (xhr) {
                        if (xhr.status === 404) {
                            notyf.error('Không tìm thấy phòng phù hợp với phương pháp điều trị này');
                        } else {
                            notyf.error('Không thể tải danh sách phòng');
                        }
                    });
                }
            });
        }).catch(error => {
            notyf.error('Không thể tải thông tin điều trị');
        });
    }).catch(error => {
        notyf.error('Không thể xác thực thông tin bác sĩ');
    });
}

function editAssignment(code) {
    // Đảm bảo có thông tin bác sĩ trước
    ensureCurrentDoctor().then(() => {
        $.get(`/api/Utils/GetAssignment/${code}`, function (assignment) {

            if (!assignment) {
                notyf.error('Không tìm thấy thông tin phân công');
                return;
            }

            // Check if current doctor has permission
            if (!currentDoctor || assignment.doctorName !== currentDoctor.name) {
                notyf.error('Bạn không có quyền chỉnh sửa bản ghi của bác sĩ khác');
                return;
            }

            $.confirm({
                title: 'Chỉnh sửa phân công',
                content: `
                    <form id="assignmentEditForm">
                        <div class="flex gap-4 mb-4">
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Mã phân công</label>
                                <input type="text" class="form-input w-full" value="${assignment.code}" readonly>
                            </div>
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Bác sĩ phân công</label>
                                <input type="text" class="form-input w-full" value="${assignment.doctorName}" readonly>
                            </div>
                        </div>
                        <div class="flex gap-4 mb-4">
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Ngày bắt đầu</label>
                                <input type="text" class="form-input w-full" id="assignmentStartDate" name="assignmentStartDate" value="${assignment.startDate}">
                            </div>
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Ngày kết thúc</label>
                                <input type="text" class="form-input w-full" id="assignmentEndDate" name="assignmentEndDate" value="${assignment.endDate}">
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="font-semibold mb-2">Ghi chú</label>
                            <textarea class="form-textarea w-full" rows="3">${assignment.note || 'Không có ghi chú'}</textarea>
                        </div>
                        <div id="assignmentEditError" style="display:none; color:#dc3545; font-weight:bold;"></div>
                    </form>`,
                type: 'blue',
                typeAnimated: true,
                theme: 'modern',
                columnClass: 'medium',
                boxWidth: '1000px',
                useBootstrap: false,
                buttons: {
                    save: {
                        text: 'Lưu',
                        btnClass: 'btn-primary',
                        action: function () {
                            // Ẩn cảnh báo cũ
                            $("#assignmentEditError").hide().text('');

                            if (!$("#assignmentEditForm").valid()) {
                                $("#assignmentEditError").text("Vui lòng kiểm tra lại thông tin ngày tháng.").show();
                                return false;
                            }
                            const startDate = this.$content.find('#assignmentStartDate').val();
                            const endDate = this.$content.find('#assignmentEndDate').val();
                            const note = this.$content.find('textarea').val();

                            if (!startDate || !endDate) {
                                notyf.error('Vui lòng nhập đầy đủ ngày bắt đầu và kết thúc');
                                return false;
                            }

                            // Gửi request cập nhật
                            $.ajax({
                                url: '/api/Utils/UpdateAssignment',
                                type: 'POST',
                                contentType: 'application/json',
                                data: JSON.stringify({
                                    code: code,
                                    startDate: startDate,
                                    endDate: endDate,
                                    note: note || ''
                                }),
                                success: function (response) {
                                    // Cập nhật thông tin trên bảng
                                    const row = $(`tr:has(td:contains('${code}'))`);
                                    row.find('td:eq(3)').text(response.startDate);
                                    row.find('td:eq(4)').text(response.endDate);

                                    // Cập nhật nút xem ghi chú
                                    const noteButton = row.find('td:eq(5)').find('button');
                                    setupNoteButton(noteButton, response.code, response.note);

                                    // Hiển thị thông báo thành công
                                    notyf.success('Đã cập nhật thông tin phân công');
                                },
                                error: function (xhr) {
                                    notyf.error(xhr.responseText || 'Có lỗi xảy ra khi cập nhật thông tin');
                                }
                            });
                        }
                    },
                    close: {
                        text: 'Đóng',
                        btnClass: 'btn-default'
                    }
                },
                onContentReady: function () {
                    $("#assignmentEditForm").validate({
                        rules: {
                            assignmentStartDate: {
                                required: true,
                                dateFormat: true
                            },
                            assignmentEndDate: {
                                required: true,
                                dateFormat: true,
                                endDateAfterStartDate: true
                            }
                        },
                        messages: {
                            assignmentStartDate: {
                                required: "Vui lòng nhập ngày bắt đầu",
                                dateFormat: "Ngày phải có định dạng dd/MM/yyyy"
                            },
                            assignmentEndDate: {
                                required: "Vui lòng nhập ngày kết thúc",
                                dateFormat: "Ngày phải có định dạng dd/MM/yyyy",
                                endDateAfterStartDate: "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu"
                            }
                        },
                        errorElement: "div",
                        errorClass: "text-danger",
                        highlight: function (element) {
                            $(element).addClass("border-red-500");
                        },
                        unhighlight: function (element) {
                            $(element).removeClass("border-red-500");
                        },
                        errorPlacement: function (error, element) {
                            error.insertAfter(element);
                        }
                    });

                    // Custom method giống như bạn dùng cho treatment record
                    $.validator.addMethod("dateFormat", function (value, element) {
                        return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
                    }, "Ngày phải có định dạng dd/MM/yyyy.");

                    $.validator.addMethod("endDateAfterStartDate", function (value, element) {
                        var startDate = $("#assignmentStartDate").val();
                        if (!startDate || !value) return true;
                        var startParts = startDate.split("/");
                        var endParts = value.split("/");
                        var start = new Date(startParts[2], startParts[1] - 1, startParts[0]);
                        var end = new Date(endParts[2], endParts[1] - 1, endParts[0]);
                        return end >= start;
                    }, "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
                }
            });
        }).catch(error => {
            notyf.error('Không thể xác thực thông tin bác sĩ');
        });
    }).catch(error => {
        notyf.error('Không thể xác thực thông tin bác sĩ');
    });
}

// Validate quy định khi nhấn Lưu
function validateRegulations() {
    let isValid = true;
    // Xóa lỗi cũ và viền đỏ cũ
    $('.regulation-error').remove();
    $('.regulation-select, .regulation-date').removeClass('border-red-500');

    // Duyệt qua từng dòng quy định
    $('[name^="Regulations["]').each(function () {
        const $row = $(this).closest('.grid');
        const index = $(this).attr('name').match(/Regulations\[(\d+)\]/)[1];
        const $regulationSelect = $('#regulationId-' + index);
        const $executionDate = $('#executionDate-' + index);

        // Kiểm tra chọn quy định
        if ($regulationSelect.length && !$regulationSelect.val()) {
            isValid = false;
            if ($regulationSelect.next('.regulation-error').length === 0) {
                $regulationSelect.after('<div class="text-danger regulation-error">Vui lòng chọn quy định</div>');
            }
            $regulationSelect.addClass('border-red-500');
            // Nếu chưa chọn quy định thì KHÔNG kiểm tra ngày thực hiện
            return;
        }
        // Kiểm tra ngày thực hiện (chỉ khi đã chọn quy định)
        if ($executionDate.length && !$executionDate.val()) {
            isValid = false;
            if ($executionDate.next('.regulation-error').length === 0) {
                $executionDate.after('<div class="text-danger regulation-error">Vui lòng chọn ngày thực hiện</div>');
            }
            $executionDate.addClass('border-red-500');
        }
    });
    return isValid;
}

$(document).on('change input', '.regulation-select, .regulation-date', function () {
    if ($(this).val()) {
        $(this).removeClass('border-red-500');
        $(this).next('.regulation-error').remove();
    }
});

document.addEventListener('DOMContentLoaded', function () {
    var hiInput = document.getElementById('HealthInsuranceNumber');
    if (hiInput) {
        hiInput.addEventListener('input', function () {
            this.value = this.value.toUpperCase();
        });
    }
});