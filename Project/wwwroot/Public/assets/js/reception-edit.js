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
                text: 'ĐÓNG',
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
        allRegulations: [],
        isLoading: false,

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

            this.setupChoices();
            this.setupFlatpickr();
            this.setupDropzone();
            this.setupValidation();
            this.setupDateListeners();
            this.updateRegulationSelectsState();

            const healthInsuranceNumber = document.querySelector('[name="Patient.HealthInsuranceNumber"]')?.value;
            const healthInsuranceCode = document.querySelector('[name="Patient.HealthInsuranceCode"]')?.value;
            this.hasHealthInsurance = !!(healthInsuranceNumber || healthInsuranceCode);

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

            isInitialized = true;
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

                // Auto-fill assignment start date with treatment start date
                if (treatmentStartDate) {
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
                        const previewContainer = document.querySelector('.dz-preview-container');
                        const messageElement = this.element.querySelector('.dz-message');
                        if (previewContainer) previewContainer.style.display = 'flex';
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
            // Append treatment record data
            formData.append('TreatmentRecord.Code', document.getElementById('treatmentRecordCode').value);
            formData.append('TreatmentRecord.Diagnosis', document.getElementById('Diagnosis').value);
            formData.append('TreatmentRecord.StartDate', document.getElementById('StartDate').value);
            formData.append('TreatmentRecord.EndDate', document.getElementById('EndDate').value);
            formData.append('TreatmentRecord.Note', document.getElementById('treatmentRecordNote').value);

            // Append patient data
            formData.append('Patient.Code', document.getElementById('Code').value);
            formData.append('Patient.Name', document.getElementById('Name').value);
            formData.append('Patient.DateOfBirth', document.getElementById('DateOfBirth').value);
            formData.append('Patient.Gender', document.getElementById('Gender').value);
            formData.append('Patient.IdentityNumber', document.getElementById('IdentityNumber').value);
            formData.append('Patient.PhoneNumber', document.getElementById('PhoneNumber').value);
            formData.append('Patient.Address', document.getElementById('Address').value);
            formData.append('Patient.Email', document.getElementById('Email').value);

            // Append health insurance data if exists
            const hasHealthInsurance = document.getElementById('HasHealthInsurance').checked;
            formData.append('Patient.HasHealthInsurance', hasHealthInsurance);

            if (hasHealthInsurance) {
                formData.append('Patient.HealthInsuranceCode', document.getElementById('HealthInsuranceCode').value);
                formData.append('Patient.HealthInsuranceNumber', document.getElementById('HealthInsuranceNumber').value);
                formData.append('Patient.HealthInsuranceExpiryDate', document.getElementById('HealthInsuranceExpiryDate').value);
                formData.append('Patient.HealthInsurancePlaceOfRegistration', document.getElementById('HealthInsurancePlaceOfRegistration').value);
                formData.append('Patient.HealthInsuranceIsRightRoute', document.getElementById('HealthInsuranceIsRightRoute').checked);
            }

        },

        handleResponse(response) {
            if (response.success) {
                notyf.success(response.message);
                setTimeout(() => {
                    window.location.href = '/Staff/TreatmentRecords';
                }, 1500);
            } else {
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

            $.validator.addMethod("over14", function (value, element) {
                var dob = $("#TreatmentRecord.StartDate").val();
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
            }, "Bệnh nhân phải trên 14 tuổi để yêu cầu CCCD.");

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
                                id: function () { return $("#healthInsuranceId").val(); }
                            },
                            dataFilter: function (data) {
                                try {
                                    const response = JSON.parse(data);
                                    if (response.success) {
                                        return response.isUnique === true;
                                    } else {
                                        notyf.error(response.message || "Lỗi khi kiểm tra tên.");
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
                        required: true,
                        maxlength: 500
                    },
                    "Patient.EmailAddress": {
                        email: true
                    },
                    "Patient.PhoneNumber": {
                        required: true,
                        minlength: 10,
                        maxlength: 15,
                        phone: true
                    },
                    "TreatmentRecord.StartDate": {
                        required: true,
                        dateFormat: true,
                        notPastDate: true
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
                        customPattern: /^[0-9A-Z]*$/,
                        remote: {
                            url: "/api/validation/healthinsurance/check",
                            type: "GET",
                            data: {
                                type: "numberhealthinsurance",
                                entityType: "healthinsurance",
                                value: function () { return $("#HealthInsuranceNumber").val(); },
                                id: function () { return $("#healthInsuranceId").val(); }
                            },
                            dataFilter: function (data) {
                                try {
                                    const response = JSON.parse(data);
                                    if (response.success) {
                                        return response.isUnique === true;
                                    } else {
                                        notyf.error(response.message || "Lỗi khi kiểm tra tên.");
                                        return false;
                                    }
                                } catch (e) {
                                    notyf.error("Lỗi kết nối server.");
                                    return false;
                                }
                            }
                        },
                        minlength: 15,
                        maxlength: 15
                    },
                    "Patient.HealthInsuranceExpiryDate": {
                        required: () => $('#HasHealthInsurance').is(':checked'),
                        dateFormat: true,
                        notExpired: true
                    },
                    "Patient.HealthInsurancePlaceOfRegistration": {
                        required: () => $('#HasHealthInsurance').is(':checked')
                    },
                    "NewTreatmentRecordDetail.TreatmentMethodId": {
                        required: function () {
                            return $("#showAddTreatmentForm").is(":visible");
                        }
                    },
                    "NewTreatmentRecordDetail.RoomId": {
                        required: function () {
                            return $("#showAddTreatmentForm").is(":visible");
                        }
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
                        required: "Địa chỉ không được bỏ trống",
                        maxlength: "Địa chỉ không được vượt quá 500 ký tự"
                    },
                    "Patient.EmailAddress": {
                        email: "Email không hợp lệ"
                    },
                    "Patient.PhoneNumber": {
                        required: "Số điện thoại không được bỏ trống",
                        minlength: "Số điện thoại phải có ít nhất 10 ký tự",
                        maxlength: "Số điện thoại không được vượt quá 15 ký tự",
                        phone: "Số điện thoại không hợp lệ"
                    },
                    "TreatmentRecord.StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống",
                        dateFormat: "Ngày bắt đầu không hợp lệ",
                        notPastDate: "Ngày bắt đầu không được là ngày trong quá khứ"
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
                        minlength: "Số BHYT có ít nhất 15 số",
                        maxlength: "Số BHYT không được vượt quá 15 số",
                        customPattern: "Số BHYT chỉ được nhập số và chữ.",
                        remote: "Số BHYT không hợp lệ"
                    },
                    "Patient.HealthInsuranceExpiryDate": {
                        required: "Ngày hết hạn không được bỏ trống",
                        dateFormat: "Ngày hết hạn không hợp lệ",
                        notExpired: "Thẻ BHYT đã hết hạn"
                    },
                    "Patient.HealthInsurancePlaceOfRegistration": {
                        required: "Nơi đăng ký không được bỏ trống"
                    },
                    "NewTreatmentRecordDetail.TreatmentMethodId": {
                        required: "Vui lòng chọn phương pháp điều trị"
                    },
                    "NewTreatmentRecordDetail.RoomId": {
                        required: "Vui lòng chọn phòng điều trị"
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
            const form = document.getElementById('receptionForm');
            if ($(form).valid()) {
                const overlay = document.getElementById('loadingOverlay');
                overlay.style.display = 'flex';

                if (this.dropzone.files.length > 0 && this.dropzone.getQueuedFiles().length > 0) {
                    this.dropzone.processQueue();
                } else {
                    const formData = new FormData(form);
                    fetch('/Staff/Receptions/Edit', {
                        method: 'POST',
                        body: formData
                    })
                        .then(response => response.json())
                        .then(this.handleResponse)
                        .catch(error => {
                            overlay.style.display = 'none';
                            notyf.error("Có lỗi xảy ra khi gửi yêu cầu: " + error);
                        });
                }
            } else {
                notyf.error("Vui lòng kiểm tra lại thông tin đã nhập");
            }
        },

        /**
         * Generate a random code
         */
        generateCode() {
            return 'REG' + Math.random().toString(36).substr(2, 9).toUpperCase();
        },

        /**
         * Get available regulations that can be added
         */
        getAvailableRegulationsForAdd() {
            const treatmentStartDate = document.getElementById('treatmentRecordStartDate').value;
            const treatmentEndDate = document.getElementById('treatmentRecordEndDate').value;

            if (!treatmentStartDate || !treatmentEndDate) {
                notyf.error('Vui lòng chọn thời gian điều trị trước khi thêm quy định');
                return [];
            }

            // Convert dates to Date objects for comparison
            const startDate = this.parseVietnameseDate(treatmentStartDate);
            const endDate = this.parseVietnameseDate(treatmentEndDate);

            // Get existing regulation IDs to exclude
            const existingRegulationIds = Array.from(document.querySelectorAll('[data-regulation-code]'))
                .map(el => el.querySelector('select').value);

            // Filter regulations that are:
            // 1. Active during treatment period
            // 2. Not already added
            return this.allRegulations.filter(regulation => {
                if (existingRegulationIds.includes(regulation.id)) {
                    return false;
                }

                const regulationStartDate = this.parseVietnameseDate(regulation.effectiveStartDate);
                const regulationEndDate = this.parseVietnameseDate(regulation.effectiveEndDate);

                return regulationStartDate <= endDate && regulationEndDate >= startDate;
            });
        },

        parseVietnameseDate(dateStr) {
            // Convert dd/MM/yyyy to Date object
            const [day, month, year] = dateStr.split('/').map(Number);
            return new Date(year, month - 1, day);
        },

        addRegulation() {
            const availableRegulations = this.getAvailableRegulationsForAdd();

            if (availableRegulations.length === 0) {
                notyf.error('Không còn quy định hợp lệ để thêm');
                return;
            }

            // Clone template
            const template = document.getElementById('regulationTemplate');
            const newRegulation = template.content.cloneNode(true);

            // Get select element from cloned template
            const select = newRegulation.querySelector('.regulation-select');

            // Add options
            availableRegulations.forEach(regulation => {
                const option = document.createElement('option');
                option.value = regulation.id;
                option.textContent = `${regulation.name} (${regulation.effectiveStartDate} - ${regulation.effectiveEndDate})`;
                select.appendChild(option);
            });

            // Generate a unique code for the new regulation
            const newRegulationCode = this.generateCode();
            const regulationDiv = newRegulation.querySelector('.grid');
            regulationDiv.setAttribute('data-regulation-code', newRegulationCode);

            // Add to list
            document.getElementById('regulationsList').appendChild(newRegulation);

            // Get treatment record dates
            const treatmentStartDate = document.getElementById('treatmentRecordStartDate').value;
            const treatmentEndDate = document.getElementById('treatmentRecordEndDate').value;

            // Initialize flatpickr for the new date input
            const dateInput = newRegulation.querySelector('.regulation-date');
            if (dateInput) {
                flatpickr(dateInput, {
                    dateFormat: "d/m/Y",
                    minDate: treatmentStartDate,
                    maxDate: treatmentEndDate,
                    required: true,
                    allowInput: true,
                    onChange: (selectedDates, dateStr) => {
                        if (selectedDates.length > 0) {
                            dateInput.classList.remove('error');
                        }
                    }
                });

                // Add change event to select
                select.addEventListener('change', () => {
                    if (select.value) {
                        select.classList.remove('error');
                    }
                });
            }
        },

        removeRegulation(code) {
            $.confirm({
                title: 'Xác nhận',
                content: 'Bạn có chắc chắn muốn xóa quy định này không?',
                type: 'red',
                typeAnimated: true,
                theme: 'modern',
                columnClass: 'medium',
                boxWidth: '400px',
                useBootstrap: false,
                buttons: {
                    confirm: {
                        text: 'Xóa',
                        btnClass: 'btn-red',
                        action: function () {
                            const regulationElement = document.querySelector(`[data-regulation-code="${code}"]`);
                            if (regulationElement) {
                                regulationElement.remove();
                            }
                            notyf.success('Xóa quy định thành công');
                        }
                    },
                    cancel: {
                        text: 'Hủy',
                        btnClass: 'btn-default'
                    }
                }
            });
        },

        removeNewRegulation(element) {
            $.confirm({
                title: 'Xác nhận',
                content: 'Bạn có chắc chắn muốn xóa quy định này không?',
                type: 'red',
                typeAnimated: true,
                theme: 'modern',
                columnClass: 'medium',
                boxWidth: '400px',
                useBootstrap: false,
                buttons: {
                    confirm: {
                        text: 'Xóa',
                        btnClass: 'btn-red',
                        action: function () {
                            const container = element.closest('[data-regulation-code]');
                            if (container) {
                                container.remove();
                            }
                            notyf.success('Xóa quy định thành công');
                        }
                    },
                    cancel: {
                        text: 'Hủy',
                        btnClass: 'btn-default'
                    }
                }
            });
        },

        getAvailableRegulations(currentIndex) {
            const existingRegulationIds = this.regulations
                .map((r, index) => index !== currentIndex ? r.RegulationId : null)
                .filter(id => id !== null);
            return this.allRegulations.filter(r => !existingRegulationIds.includes(r.id));
        },

        initRegulationDatePicker(element, selectedDate) {
            flatpickr(element, {
                dateFormat: "d/m/Y",
                defaultDate: selectedDate,
                onChange: (dateStr) => {
                    this.onRegulationChange(this.regulations.findIndex(r => r.RegulationId === dateStr), dateStr);
                }
            });
        },

        onRegulationChange(index, value) {
            if (index !== -1 && value) {
                const regulation = this.regulations.find(r => r.RegulationId === value);
                if (regulation) {
                    regulation.ExecutionDate = value;
                }
            }
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

            if (!detail) {
                notyf.error('Không tìm thấy thông tin điều trị');
                return;
            }

            if (!currentDoctor || detail.doctorName !== currentDoctor.name) {
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
                    <form>
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
                    <form>
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
                                <input type="text" class="form-input w-full flatpickr" id="assignmentStartDate" value="${assignment.startDate}">
                            </div>
                            <div class="form-group flex-1">
                                <label class="font-semibold mb-2">Ngày kết thúc</label>
                                <input type="text" class="form-input w-full flatpickr" id="assignmentEndDate" value="${assignment.endDate}">
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="font-semibold mb-2">Ghi chú</label>
                            <textarea class="form-textarea w-full" rows="3">${assignment.note || 'Không có ghi chú'}</textarea>
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
                    const startDateInput = this.$content.find('#assignmentStartDate');
                    const endDateInput = this.$content.find('#assignmentEndDate');

                    // Initialize flatpickr for date inputs
                    flatpickr(startDateInput, {
                        dateFormat: "d/m/Y",
                        allowInput: true
                    });

                    flatpickr(endDateInput, {
                        dateFormat: "d/m/Y",
                        allowInput: true
                    });
                }
            });
        }).catch(error => {
            console.error('Error ensuring current doctor:', error);
            notyf.error('Không thể xác thực thông tin bác sĩ');
        });
    }).catch(error => {
        console.error('Error ensuring current doctor:', error);
        notyf.error('Không thể xác thực thông tin bác sĩ');
    });
}