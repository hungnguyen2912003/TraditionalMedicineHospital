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
        tab: 'treatment-record',
        showAddTreatmentForm: false,
        showAddAssignmentForm: false,
        TreatmentRecordDetails: window.treatmentRecordDetailsData || [],
        Assignments: window.assignmentsData || [],
        currentEmployeeId: window.currentEmployeeId || '',
        treatmentRecordId: document.getElementById('treatmentRecordId')?.value || '',
        regulations: (window.initialRegulations || []).filter(Boolean),

        // Sử dụng hàm displayNote toàn cục thông qua wrapper
        displayNote(note, type, identifier) {
            // Ngăn chặn sự kiện click từ Alpine nếu đã có event handler jQuery
            if (type === 'treatment' && noteStorage.has(identifier)) {
                return;
            }
            displayNote(note, type, identifier);
        },

        areTreatmentDatesSelected() {
            const startDateElement = document.getElementById('StartDate');
            const endDateElement = document.getElementById('EndDate');

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
            const startDateInput = document.getElementById('StartDate');
            const endDateInput = document.getElementById('EndDate');

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
                this.regulations = window.initialRegulations.filter(Boolean).map(r => {
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
            this.setupValidation();
            this.setupDateListeners();
            this.updateRegulationSelectsState();
            this.setupCleave();

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

            this.$nextTick(() => {
                this.initRegulationDatePickers();
            });

            isInitialized = true;
        },

        setupCleave() {
            new Cleave('.advance-payment', {
                numeral: true,
                numeralThousandsGroupStyle: 'thousand',
                numeralDecimalScale: 0,
                numeralPositiveOnly: true,
                onValueChanged: function (e) {
                    $('#AdvancePayment').trigger('change');
                }
            });
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
                treatmentStartDate: document.getElementById('StartDate'),
                treatmentEndDate: document.getElementById('EndDate'),
                assignmentStartDate: document.getElementById('assignmentStartDate'),
                assignmentEndDate: document.getElementById('assignmentEndDate'),
                assignmentDateWarning: document.getElementById('assignmentDateWarning')
            };

            const treatmentStartDate = elements.treatmentStartDate.value;
            const treatmentEndDate = elements.treatmentEndDate.value;

            elements.assignmentStartDate.value = treatmentStartDate;
            elements.assignmentEndDate.value = treatmentEndDate;
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

        appendFormData(formData) {
            // Treatment record information
            formData.append('TreatmentRecord.Id', document.getElementById('treatmentRecordId')?.value || '');
            formData.append('TreatmentRecord.PatientId', document.querySelector('[name="PatientId"]')?.value || '');
            formData.append('TreatmentRecord.Code', document.getElementById('treatmentRecordCode')?.value || '');
            formData.append('TreatmentRecord.Diagnosis', document.getElementById('Diagnosis')?.value || '');
            formData.append('TreatmentRecord.StartDate', document.getElementById('StartDate')?.value || '');
            formData.append('TreatmentRecord.EndDate', document.getElementById('EndDate')?.value || '');
            formData.append('TreatmentRecord.Note', document.getElementById('treatmentRecordNote')?.value || '');
            formData.append('TreatmentRecord.AdvancePayment', document.getElementById('AdvancePayment')?.value || '');

            const treatmentTableComponent = document.querySelector('[x-data="treatmentDetailTable"]');
            const details = treatmentTableComponent && Alpine.$data(treatmentTableComponent).details || [];
            details.forEach((detail, idx) => {
                formData.append(`TreatmentRecordDetails[${idx}].Code`, detail.Code || '');
                formData.append(`TreatmentRecordDetails[${idx}].TreatmentMethodId`, detail.TreatmentMethodId || '');
                formData.append(`TreatmentRecordDetails[${idx}].RoomId`, detail.RoomId || '');
                formData.append(`TreatmentRecordDetails[${idx}].Note`, detail.Note || '');
            });

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
                    window.location.href = '/phieu-dieu-tri';
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

                var treatmentStartDate = $("input[name='StartDate']").val();
                var treatmentEndDate = $("input[name='EndDate']").val();

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

                var treatmentStartDate = $("input[name='StartDate']").val();
                var treatmentEndDate = $("input[name='EndDate']").val();
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

            $.validator.addMethod("numberWithComma", function (value, element) {
                var cleanValue = value.replace(/,/g, '');
                return this.optional(element) || !isNaN(cleanValue) && cleanValue >= 0;
            }, "Tiền ứng trước phải là số.");

            $.validator.addMethod("minWithComma", function (value, element, param) {
                var cleanValue = value.replace(/,/g, '');
                return this.optional(element) || !isNaN(cleanValue) && Number(cleanValue) >= param;
            }, "Tiền ứng trước phải lớn hơn 0.");

            $("#editForm").validate({
                ignore: [],
                rules: {
                    "StartDate": {
                        required: true,
                        dateFormat: true
                    },
                    "EndDate": {
                        required: true,
                        dateFormat: true,
                        endDateAfterStartDate: true
                    },
                    "AdvancePayment": {
                        required: true,
                        numberWithComma: true,
                        minWithComma: 1
                    },
                    "Diagnosis": {
                        required: true
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
                    "StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống",
                        dateFormat: "Ngày bắt đầu không hợp lệ"
                    },
                    "EndDate": {
                        required: "Ngày kết thúc không được bỏ trống",
                        dateFormat: "Ngày kết thúc không hợp lệ",
                        endDateAfterStartDate: "Ngày kết thúc phải sau ngày bắt đầu"
                    },
                    "AdvancePayment": {
                        required: "Tiền ứng trước không được bỏ trống.",
                        numberWithComma: "Tiền ứng trước không hợp lệ.",
                        minWithComma: "Tiền ứng trước phải lớn hơn 0."
                    },
                    "Diagnosis": {
                        required: "Chẩn đoán không được bỏ trống"
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
            $("#editForm").on('submit', function (e) {
                e.preventDefault();
                self.update();
            });
        },

        update() {
            // Kiểm tra validate client trước khi submit
            if (!$('#editForm').valid()) {
                notyf.error('Vui lòng kiểm tra thông tin');
                return;
            }
            // Trước khi validate, xóa hết viền đỏ cũ
            this.regulations.forEach((reg, idx) => {
                const select = document.getElementById('regulationId-' + idx);
                const dateInput = document.getElementById('executionDate-' + idx);
                if (select) select.classList.remove('border-red-500');
                if (dateInput) dateInput.classList.remove('border-red-500');
            });
            // Kiểm tra validate các dòng quy định điều trị
            let regulationValid = true;
            this.regulations.forEach((reg, idx) => {
                const select = document.getElementById('regulationId-' + idx);
                const dateInput = document.getElementById('executionDate-' + idx);
                // Validate chọn quy định
                if (!reg.RegulationId) {
                    if (select) select.classList.add('border-red-500');
                    regulationValid = false;
                }
                // Validate ngày thực hiện (nếu đã chọn quy định)
                if (reg.RegulationId && !reg.ExecutionDate) {
                    if (dateInput) dateInput.classList.add('border-red-500');
                    regulationValid = false;
                }
            });

            // Kiểm tra validate chi tiết điều trị
            let detailValid = true;
            document.querySelectorAll('select[name^="TreatmentRecordDetails"][name$=".TreatmentMethodId"], select[name^="TreatmentRecordDetails"][name$=".RoomId"]')
                .forEach(select => select.classList.remove('border-red-500'));

            document.querySelectorAll('select[name^="TreatmentRecordDetails"][name$=".TreatmentMethodId"]')
                .forEach((methodSelect, idx) => {
                    const roomSelect = document.querySelector(`select[name='TreatmentRecordDetails[${idx}].RoomId']`);
                    if (!methodSelect.value) {
                        methodSelect.classList.add('border-red-500');
                        detailValid = false;
                    }
                    if (methodSelect.value && (!roomSelect || !roomSelect.value)) {
                        if (roomSelect) roomSelect.classList.add('border-red-500');
                        detailValid = false;
                    }
                });

            if (!regulationValid && !detailValid) {
                notyf.error('Vui lòng kiểm tra thông tin quy định điều trị và chi tiết điều trị');
                return;
            }
            if (!regulationValid) {
                notyf.error('Vui lòng kiểm tra thông tin quy định điều trị');
                return;
            }
            if (!detailValid) {
                notyf.error('Vui lòng kiểm tra thông tin chi tiết điều trị');
                return;
            }

            // Lấy thông tin bác sĩ hiện tại
            const currentEmployeeId = (window.currentEmployeeId || '').toLowerCase();
            const currentEmployeeCode = (window.currentEmployeeCode || '').toLowerCase();
            // Lấy danh sách phân công hiện tại
            const hasAssignment = (window.assignmentsData || []).some(a => (a.employeeId || '').toLowerCase() === currentEmployeeId);
            // Lấy danh sách chi tiết điều trị hiện tại (từ bảng trên giao diện)
            const treatmentTableComponent = document.querySelector('[x-data="treatmentDetailTable"]');
            const details = treatmentTableComponent && Alpine.$data(treatmentTableComponent).details || [];
            const hasTreatment = details.some(d => (d.CreatedBy || '').toLowerCase() === currentEmployeeCode);

            if (!hasAssignment && hasTreatment && !this.showAddAssignmentForm) {
                notyf.error("Bạn cần bổ sung thông tin phân công");
                return;
            }
            if (hasAssignment && !hasTreatment && !this.showAddTreatmentForm) {
                notyf.error("Bạn cần bổ sung thông tin điều trị");
                return;
            }

            const form = document.getElementById('editForm');
            if (!form) {
                notyf.error("Không tìm thấy form. Vui lòng tải lại trang.");
                return;
            }


            try {
                this.overlay.style.display = 'flex';

                const formData = new FormData();

                // Append other form data
                this.appendFormData(formData);

                // Log dữ liệu gửi lên server
                for (let pair of formData.entries()) {
                    console.log('FormData:', pair[0], pair[1]);
                }

                fetch('/phieu-dieu-tri/cap-nhat-phieu/' + this.treatmentRecordId, {
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

        goBack() {
            window.location.href = '/bac-si';
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
        },
        addRegulation() {
            if (this.regulations.length >= 5) {
                notyf.error('Không thể thêm quá 5 quy định cho một phiếu điều trị');
                return;
            }
            this.regulations.push({
                Code: this.generateCode(),
                RegulationId: '',
                ExecutionDate: '',
                Note: ''
            });
            this.$nextTick(() => {
                this.initRegulationDatePickers();
            });
        },
        removeRegulation(idx) {
            this.regulations.splice(idx, 1);
        },
        getAvailableRegulations(idx) {
            // Lấy danh sách id đã chọn ở các dòng khác
            const selectedIds = this.regulations
                .map((r, i) => i !== idx ? r.RegulationId : null)
                .filter(id => id);
            // Lọc regulationsData để loại bỏ các id đã chọn
            return (window.regulationsData || []).filter(r => !selectedIds.includes(r.id.toString()));
        },
        initRegulationDatePickers() {
            this.regulations.forEach((reg, idx) => {
                const dateInput = document.getElementById('executionDate-' + idx);
                if (dateInput) {
                    if (dateInput._flatpickr) {
                        dateInput._flatpickr.destroy();
                    }
                    flatpickr(dateInput, {
                        dateFormat: 'd/m/Y',
                        allowInput: true,
                        onChange: (selectedDates, dateStr) => {
                            this.regulations[idx].ExecutionDate = dateStr;
                        }
                    });
                }
            });
        }
    }));

    Alpine.data('treatmentDetailTable', () => ({
        details: [],
        currentEmployeeCode: window.currentEmployeeCode,
        treatmentMethods: window.treatmentMethodsData || [],
        allRooms: window.roomsData || [],
        init() {
            if (window.treatmentRecordDetailsData && Array.isArray(window.treatmentRecordDetailsData)) {
                this.details = window.treatmentRecordDetailsData.map(d => {
                    let roomId = (d.roomId || d.RoomId || '').toString();
                    let methodId = (d.treatmentMethodId || d.TreatmentMethodId || '').toString();
                    if (!methodId && roomId && window.roomsData) {
                        let room = window.roomsData.find(r => r.id.toString() === roomId);
                        methodId = room ? (room.treatmentMethodId || room.TreatmentMethodId || '').toString() : '';
                    }
                    return {
                        Code: d.code || d.Code || '',
                        TreatmentMethodId: methodId,
                        RoomId: roomId,
                        Note: d.note || d.Note || '',
                        CreatedBy: d.createdBy || d.CreatedBy || ''
                    };
                });
                // Thêm dòng này để đảm bảo AlpineJS đồng bộ lại select
                this.$nextTick(() => { });
            }
        },
        addDetail() {
            this.details.push({
                Code: this.generateCode(),
                TreatmentMethodId: '',
                RoomId: '',
                Note: '',
                CreatedBy: window.currentEmployeeCode
            });
            this.$nextTick(() => {
                this.updateRoomCursor(this.details.length - 1);
            });
        },
        removeDetail(idx) {
            if (this.details[idx].CreatedBy === this.currentEmployeeCode) {
                this.details.splice(idx, 1);
                this.$nextTick(() => {
                    this.details.forEach((_, i) => this.updateRoomCursor(i));
                });
            }
        },
        onMethodChange(idx) {
            this.details[idx].RoomId = '';
            this.$nextTick(() => {
                this.updateRoomCursor(idx);
            });
        },
        getRoomsForMethod(methodId, idx = null) {
            if (!methodId) {
                return [];
            }
            let rooms = this.allRooms.filter(r => r.treatmentMethodId == methodId);
            // Đảm bảo phòng đang chọn luôn có trong danh sách
            if (idx !== null) {
                const currentRoomId = this.details[idx]?.RoomId;
                if (currentRoomId && !rooms.some(r => r.id == currentRoomId)) {
                    const currentRoom = this.allRooms.find(r => r.id == currentRoomId);
                    if (currentRoom) rooms = [currentRoom, ...rooms];
                }
            }
            return rooms;
        },
        isRoomDisabled(detail) {
            return !detail.TreatmentMethodId;
        },
        generateCode() {
            const chars = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
            let code = '';
            for (let i = 0; i < 8; i++) code += chars.charAt(Math.floor(Math.random() * chars.length));
            return code;
        },
        getAvailableMethods(currentIdx) {
            // Nếu là dòng mới do bác sĩ hiện tại tạo
            const isNew = !this.details[currentIdx].CreatedBy || this.details[currentIdx].CreatedBy === window.currentEmployeeCode;
            const allMethods = isNew ? (window.treatmentMethodsData || []) : (window.allTreatmentMethodsData || []);

            // Lấy các phương pháp đã được chọn ở các dòng khác
            const selectedIds = this.details
                .map((d, idx) => idx !== currentIdx ? d.TreatmentMethodId : null)
                .filter(id => id);

            // Lọc ra các phương pháp chưa được chọn
            let available = allMethods.filter(m => !selectedIds.includes(m.id.toString()));

            // Luôn thêm method đang chọn nếu chưa có trong danh sách
            const currentId = this.details[currentIdx]?.TreatmentMethodId;
            if (currentId && !available.some(m => m.id.toString() === currentId)) {
                const currentMethod = allMethods.find(m => m.id.toString() === currentId);
                if (currentMethod) available = [currentMethod, ...available];
            }

            return available;
        },
        updateRoomCursor(idx) {
            const roomSelect = document.querySelector(`[name='TreatmentRecordDetails[${idx}].RoomId']`);
            if (roomSelect) {
                if (this.isRoomDisabled(this.details[idx])) {
                    roomSelect.classList.add('cursor-not-allowed');
                } else {
                    roomSelect.classList.remove('cursor-not-allowed');
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