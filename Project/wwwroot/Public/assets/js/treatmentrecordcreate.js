document.addEventListener('alpine:init', () => {
    Alpine.data('treatmentrecord', () => ({
        // State
        filteredRooms: [],
        allRooms: [],
        roomChoices: null,
        regulations: [],
        availableRegulations: [],
        errors: {},
        choicesInstances: {},
        assignmentStartPicker: null,
        assignmentEndPicker: null,

        /**
         * Initialize the component
         */
        init() {
            try {
                // Initialize arrays first
                this.filteredRooms = [];
                this.allRooms = window.roomsData || [];
                this.availableRegulations = window.regulationsData || [];

                // Initialize all components
                this.$nextTick(() => {
                    this.setupChoices();
                    this.setupDateTimePicker();
                    this.setupValidation();
                    this.setupTreatmentDateListeners();
                    // Setup room filtering
                    const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');
                    if (treatmentMethodSelect) {
                        treatmentMethodSelect.addEventListener('change', (e) => this.filterRooms(e.target.value));
                    }
                });
            } catch (error) {
                notyf.error('Có lỗi xảy ra khi khởi tạo form');
            }
        },

        /**
         * Filter rooms based on treatment method
         */
        filterRooms(treatmentMethodId) {
            if (!treatmentMethodId) {
                this.filteredRooms = [];
                if (this.roomChoices) {
                    this.roomChoices.clearStore();
                    this.roomChoices.disable();
                }
                return;
            }

            this.filteredRooms = this.allRooms.filter(room =>
                room.treatmentMethodId === treatmentMethodId
            );

            if (this.roomChoices) {
                this.roomChoices.enable();
                this.roomChoices.clearStore();
                this.roomChoices.setChoices(
                    this.filteredRooms.map(room => ({
                        value: room.id,
                        label: room.name
                    })),
                    'value',
                    'label',
                    true
                );

                // If there's only one room, select it automatically
                if (this.filteredRooms.length === 1) {
                    const roomId = this.filteredRooms[0].id;
                    const roomSelect = document.getElementById('treatmentRecordDetailRoom');

                    // Set the value directly on the select element first
                    if (roomSelect) {
                        roomSelect.value = roomId;
                    }

                    // Then update Choices.js
                    this.roomChoices.setChoiceByValue(roomId);

                    // The validation will be handled by the change event listener
                    // that we set up in initRoomChoices
                }
            }
        },

        /**
         * Generate a unique code for regulations
         * Returns an 8-character code containing numbers and letters A-Z
         */
        generateCode() {
            const chars = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
            let code = '';
            for (let i = 0; i < 8; i++) {
                code += chars.charAt(Math.floor(Math.random() * chars.length));
            }
            return code;
        },

        /**
         * Get available regulations for adding new regulation
         */
        getAvailableRegulationsForAdd() {
            const treatmentStartDate = document.getElementById('StartDate').value;
            const treatmentEndDate = document.getElementById('EndDate').value;

            if (!treatmentStartDate || !treatmentEndDate) {
                return [];
            }

            const treatmentStart = this.parseDate(treatmentStartDate);
            const treatmentEnd = this.parseDate(treatmentEndDate);

            // Get currently selected regulation IDs
            const selectedRegulationIds = this.regulations.map(r => r.RegulationId).filter(id => id);

            // Filter regulations based on date range and not already selected
            return this.availableRegulations.filter(r => {
                // Check if regulation is not already selected
                if (selectedRegulationIds.includes(r.id.toString())) {
                    return false;
                }

                // Parse regulation effective dates and format them with slashes
                const regStart = this.parseDate(r.effectiveStartDate);
                const regEnd = this.parseDate(r.effectiveEndDate);

                // Format dates for display
                r.effectiveStartDate = regStart ? `${regStart.getDate().toString().padStart(2, '0')}/${(regStart.getMonth() + 1).toString().padStart(2, '0')}/${regStart.getFullYear()}` : '';
                r.effectiveEndDate = regEnd ? `${regEnd.getDate().toString().padStart(2, '0')}/${(regEnd.getMonth() + 1).toString().padStart(2, '0')}/${regEnd.getFullYear()}` : '';

                // Check if regulation's effective period overlaps with treatment period
                // and regulation starts before treatment ends
                return regStart <= treatmentEnd && regEnd >= treatmentStart && regStart <= treatmentEnd;
            });
        },

        /**
         * Add a new regulation
         */
        addRegulation() {
            const startDate = document.getElementById('StartDate').value;
            const endDate = document.getElementById('EndDate').value;

            if (!startDate || !endDate) {
                notyf.error('Vui lòng chọn thời gian điều trị trước khi thêm quy định');
                return;
            }

            if (this.regulations.length >= 5) {
                notyf.error('Không thể thêm quá 5 quy định cho một phiếu điều trị');
                return;
            }

            // Get available valid regulations
            const availableRegs = this.getAvailableRegulationsForAdd();

            if (availableRegs.length === 0) {
                notyf.error('Không còn quy định hợp lệ để thêm');
                return;
            }

            const newRegulation = {
                Code: this.generateCode(),
                RegulationId: '',
                ExecutionDate: '',
                Note: ''
            };
            this.regulations.push(newRegulation);

            // Initialize select for the new regulation after the template is rendered
            this.$nextTick(() => {
                const index = this.regulations.length - 1;
                const selectElement = document.getElementById(`regulationId-${index}`);
                if (selectElement) {
                    // Check if treatment dates are selected
                    const hasStartDate = document.getElementById('StartDate').value.trim() !== '';
                    const hasEndDate = document.getElementById('EndDate').value.trim() !== '';

                    if (!hasStartDate || !hasEndDate) {
                        selectElement.disabled = true;
                        selectElement.classList.add('opacity-50', 'cursor-not-allowed');
                    }

                    // Get currently selected regulation IDs
                    const selectedIds = this.regulations
                        .map(r => r.RegulationId)
                        .filter(id => id !== '');

                    // Filter out already selected regulations
                    const availableChoices = availableRegs.filter(r => !selectedIds.includes(r.id.toString()));

                    // Clear existing options
                    selectElement.innerHTML = '';

                    // Add default option
                    const defaultOption = document.createElement('option');
                    defaultOption.value = '';
                    defaultOption.text = 'Chọn quy định';
                    selectElement.appendChild(defaultOption);

                    // Add available regulations
                    availableChoices.forEach(reg => {
                        const option = document.createElement('option');
                        option.value = reg.id;
                        option.text = `${reg.name} (${reg.effectiveStartDate} - ${reg.effectiveEndDate})`;
                        selectElement.appendChild(option);
                    });

                    // Add change event listener
                    selectElement.addEventListener('change', (e) => {
                        this.onRegulationChange(index, e.target.value);
                    });
                }
            });
        },

        removeRegulation(index) {
            this.regulations.splice(index, 1);
        },

        /**
         * Get available regulations for a specific select
         */
        getAvailableRegulations(currentIndex) {
            const treatmentStartDate = document.getElementById('StartDate').value;
            const treatmentEndDate = document.getElementById('EndDate').value;

            if (!treatmentStartDate || !treatmentEndDate) {
                return [];
            }

            const treatmentStart = this.parseDate(treatmentStartDate);
            const treatmentEnd = this.parseDate(treatmentEndDate);

            // Filter out already selected regulations and check date range
            const selectedIds = this.regulations
                .map((r, index) => index !== currentIndex ? r.RegulationId : null)
                .filter(id => id);

            return this.availableRegulations.filter(r => {
                // Check if regulation is not already selected
                if (selectedIds.includes(r.id.toString())) {
                    return false;
                }

                // Parse regulation effective dates
                const regStart = this.parseDate(r.effectiveStartDate);
                const regEnd = this.parseDate(r.effectiveEndDate);

                // Check if regulation's effective period overlaps with treatment period
                // and regulation starts before treatment ends
                return regStart <= treatmentEnd && regEnd >= treatmentStart && regStart <= treatmentEnd;
            });
        },

        appendFormData(formData) {
            // Get antiforgery token first
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (tokenInput) {
                formData.append('__RequestVerificationToken', tokenInput.value);
            }

            // Append treatment record data
            var el = document.getElementById('treatmentRecordCode');
            if (el) {
                var value = el.value;
                formData.append('TreatmentRecord.Code', value);
            }
            formData.append('TreatmentRecord.Diagnosis', document.getElementById('Diagnosis').value);
            formData.append('TreatmentRecord.StartDate', document.getElementById('StartDate').value);
            formData.append('TreatmentRecord.EndDate', document.getElementById('EndDate').value);
            formData.append('TreatmentRecord.PatientId', document.getElementById('PatientId').value);
            formData.append('TreatmentRecord.Note', document.getElementById('treatmentRecordNote').value);

            // Append treatment record details
            const methodSelects = document.querySelectorAll('select[name$=".TreatmentMethodId"]');
            methodSelects.forEach((methodSelect, index) => {
                const codeInput = document.getElementById(`treatmentRecordDetailCode-${index}`);
                const roomSelect = document.getElementById(`treatmentRecordDetailRoom-${index}`);
                const noteInput = document.getElementById(`treatmentRecordDetailNote-${index}`);

                if (codeInput && roomSelect && methodSelect) {
                    formData.append(`TreatmentRecordDetails[${index}].Code`, codeInput.value);
                    formData.append(`TreatmentRecordDetails[${index}].TreatmentMethodId`, methodSelect.value);
                    formData.append(`TreatmentRecordDetails[${index}].RoomId`, roomSelect.value);
                    formData.append(`TreatmentRecordDetails[${index}].Note`, noteInput ? noteInput.value : '');
                }
            });

            // Append assignment data
            formData.append('Assignment.Code', document.getElementById('assignmentCode').value);
            formData.append('Assignment.StartDate', document.getElementById('assignmentStartDate').value);
            formData.append('Assignment.EndDate', document.getElementById('assignmentEndDate').value);
            formData.append('Assignment.Note', document.getElementById('assignmentNote').value);

            // Append regulations data
            const regulationElements = document.querySelectorAll('[id^="regulationId-"]');
            regulationElements.forEach((element, index) => {
                const regulationId = element.value;
                if (regulationId) {
                    const executionDate = document.getElementById(`executionDate-${index}`).value;
                    const note = document.getElementById(`note-${index}`).value;
                    const code = document.querySelector(`[name="Regulations[${index}].Code"]`).value;

                    formData.append(`Regulations[${index}].Code`, code);
                    formData.append(`Regulations[${index}].RegulationId`, regulationId);
                    formData.append(`Regulations[${index}].ExecutionDate`, executionDate);
                    formData.append(`Regulations[${index}].Note`, note);
                }
            });
        },

        /**
         * Set up Choices.js for select elements
         */
        setupChoices() {
            const selectElements = document.querySelectorAll('select.choices');
            selectElements.forEach(select => {
                if (select.id === 'treatmentRecordDetailRoom' || select.choices) return;

                const choices = new Choices(select, {
                    searchEnabled: true,
                    searchPlaceholderValue: 'Tìm kiếm...',
                    removeItemButton: true,
                    noResultsText: 'Không tìm thấy kết quả',
                    noChoicesText: 'Không có lựa chọn nào',
                    itemSelectText: ''
                });

                this.choicesInstances[select.id] = choices;
                select.choices = choices;
            });

            this.initRoomChoices();
        },

        /**
         * Initialize Choices.js for room select
         */
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
                placeholderValue: 'Chọn phòng',
                callbackOnInit: () => {
                    // Add change event listener after initialization
                    roomSelect.addEventListener('change', (e) => {
                        const selectedValue = e.target.value;
                        // Trigger validation only if value changes by user interaction
                        if (selectedValue) {
                            $(roomSelect).valid();
                        }
                    });
                }
            });

            roomSelect.choices = this.roomChoices; // Mark as initialized
            this.roomChoices.disable();
        },

        /**
         * Set up Flatpickr for date inputs
         */
        setupDateTimePicker() {
            try {
                // Regular date pickers (except assignment dates)
                const regularDateInputs = document.querySelectorAll('.flatpickr:not(#assignmentStartDate):not(#assignmentEndDate)');
                regularDateInputs.forEach(input => {
                    if (input) {
                        flatpickr(input, {
                            dateFormat: "d/m/Y",
                            allowInput: true,
                            onChange: function (selectedDates, dateStr) {
                                // If this is the treatment start date, update assignment start date
                                if (input.id === 'StartDate') {
                                    const assignmentStartInput = document.getElementById('assignmentStartDate');
                                    if (assignmentStartInput && !assignmentStartInput.disabled) {
                                        assignmentStartInput.value = dateStr;
                                        if (this.assignmentStartPicker) {
                                            this.assignmentStartPicker.setDate(dateStr);
                                        }
                                    }
                                }
                            }
                        });
                    }
                });

                const treatmentStartDate = document.getElementById('StartDate');
                const treatmentEndDate = document.getElementById('EndDate');
                const assignmentStartInput = document.getElementById('assignmentStartDate');
                const assignmentEndInput = document.getElementById('assignmentEndDate');
                const warningMessage = document.getElementById('assignmentDateWarning');

                if (!treatmentStartDate || !treatmentEndDate || !assignmentStartInput || !assignmentEndInput) {
                    console.warn('Some date input elements are missing');
                    return;
                }

                // Function to check if treatment dates are selected
                const areTreatmentDatesSelected = () => {
                    return treatmentStartDate.value && treatmentEndDate.value;
                };

                // Function to update assignment date pickers state
                const updateAssignmentDatePickersState = () => {
                    const hasBothDates = areTreatmentDatesSelected();

                    if (warningMessage) {
                        warningMessage.style.display = hasBothDates ? 'none' : 'block';
                    }

                    if (assignmentStartInput && assignmentEndInput) {
                        assignmentStartInput.disabled = !hasBothDates;
                        assignmentEndInput.disabled = !hasBothDates;

                        if (hasBothDates) {
                            assignmentStartInput.classList.remove('opacity-50', 'cursor-not-allowed');
                            assignmentEndInput.classList.remove('opacity-50', 'cursor-not-allowed');
                            // Set assignment start date to match treatment start date
                            assignmentStartInput.value = treatmentStartDate.value;
                        } else {
                            assignmentStartInput.classList.add('opacity-50', 'cursor-not-allowed');
                            assignmentEndInput.classList.add('opacity-50', 'cursor-not-allowed');
                            assignmentStartInput.value = '';
                            assignmentEndInput.value = '';
                        }
                    }
                };

                // Watch for changes in treatment dates
                if (treatmentStartDate) {
                    treatmentStartDate.addEventListener('change', function () {
                        updateAssignmentDatePickersState();
                        if (areTreatmentDatesSelected()) {
                            if (this.assignmentStartPicker) this.assignmentStartPicker.set('minDate', this.value);
                            if (this.assignmentEndPicker) this.assignmentEndPicker.set('minDate', this.value);
                        }
                    });
                }

                if (treatmentEndDate) {
                    treatmentEndDate.addEventListener('change', function () {
                        updateAssignmentDatePickersState();
                        if (areTreatmentDatesSelected()) {
                            // Parse dates
                            const start = treatmentStartDate.value.split('/').reverse().join('-');
                            const end = treatmentEndDate.value.split('/').reverse().join('-');
                            const startDateObj = new Date(start);
                            const endDateObj = new Date(end);
                            assignmentStartInput.value = treatmentStartDate.value;
                            if (endDateObj >= startDateObj) {
                                assignmentEndInput.value = treatmentEndDate.value;
                            } else {
                                assignmentEndInput.value = '';
                            }
                        }
                    }.bind(this));
                }

                // Watch for changes in regulations array
                this.$watch('regulations', () => {
                    this.$nextTick(() => {
                        document.querySelectorAll('[id^="executionDate-"]').forEach(element => {
                            if (!element._flatpickr) {
                                this.initRegulationDatePicker(element);
                            }
                        });
                    });
                });
            } catch (error) {
                notyf.error('Có lỗi xảy ra khi khởi tạo chọn ngày');
            }
        },

        /**
         * Initialize flatpickr for regulation date fields
         */
        initRegulationDatePicker(element, regulationId) {
            if (element._flatpickr) {
                element._flatpickr.destroy();
            }

            // Get treatment end date
            const treatmentEndDate = document.getElementById('EndDate').value;
            const treatmentEnd = this.parseDate(treatmentEndDate);

            // Find the selected regulation
            const regulation = this.availableRegulations.find(r => r.id.toString() === regulationId);

            if (!regulation) {
                flatpickr(element, {
                    dateFormat: "d/m/Y",
                    allowInput: true,
                    disable: [
                        function (date) {
                            return true; // Disable all dates if no regulation is selected
                        }
                    ]
                });
                return;
            }

            // Parse effective start and end dates
            const startDate = this.parseDate(regulation.effectiveStartDate);
            const endDate = this.parseDate(regulation.effectiveEndDate);

            // Use the earlier of regulation end date and treatment end date as the max date
            const maxDate = treatmentEnd && endDate ? (treatmentEnd < endDate ? treatmentEnd : endDate) : (treatmentEnd || endDate);

            flatpickr(element, {
                dateFormat: "d/m/Y",
                allowInput: true,
                minDate: startDate,
                maxDate: maxDate,
                disable: [
                    function (date) {
                        // Disable dates outside the effective range and after treatment end date
                        return date < startDate || date > maxDate;
                    }
                ]
            });
        },

        /**
         * Parse date string (dd/mm/yyyy) to Date object
         */
        parseDate(dateStr) {
            try {
                // Handle different date formats
                if (dateStr.includes('/')) { // dd/mm/yyyy
                    const [day, month, year] = dateStr.split('/').map(Number);
                    return new Date(year, month - 1, day);
                } else if (dateStr.includes(' ') && dateStr.split(' ').length === 3) {
                    const [day, month, year] = dateStr.split(' ').map(Number);
                    return new Date(year, month - 1, day);
                } else if (dateStr.includes('-')) {
                    return new Date(dateStr);
                } else {
                    return null;
                }
            } catch (error) {
                return null;
            }
        },

        /**
         * Handle regulation selection change
         */
        onRegulationChange(index, regulationId) {
            const dateField = document.getElementById(`executionDate-${index}`);
            if (dateField) {
                // Clear any existing flatpickr instance
                if (dateField._flatpickr) {
                    dateField._flatpickr.destroy();
                }

                if (regulationId) {
                    // Initialize flatpickr only if regulation is selected
                    const treatmentStartDate = document.getElementById('StartDate').value;
                    this.initRegulationDatePicker(dateField, regulationId);

                    // Set execution date to treatment start date after regulation is selected
                    if (treatmentStartDate) {
                        this.regulations[index].ExecutionDate = treatmentStartDate;
                        if (dateField._flatpickr) {
                            dateField._flatpickr.setDate(treatmentStartDate);
                        }
                    }

                    // Update all other regulation selects
                    this.updateAllRegulationSelects();
                } else {
                    // Clear execution date if regulation is deselected
                    this.regulations[index].ExecutionDate = '';
                }
            }
        },

        /**
         * Update all regulation select boxes
         */
        updateAllRegulationSelects() {
            const availableRegs = this.getAvailableRegulationsForAdd();
            const selectedIds = this.regulations
                .map(r => r.RegulationId)
                .filter(id => id !== '');

            // Update each select box
            this.regulations.forEach((regulation, index) => {
                const selectElement = document.getElementById(`regulationId-${index}`);
                if (selectElement && !selectedIds.includes(regulation.RegulationId)) {
                    // Get available regulations excluding current selection
                    const currentSelectedId = regulation.RegulationId;
                    const availableChoices = availableRegs.filter(r =>
                        !selectedIds.includes(r.id.toString()) || r.id.toString() === currentSelectedId
                    );

                    // Store current selection
                    const currentValue = selectElement.value;

                    // Clear existing options
                    selectElement.innerHTML = '';

                    // Add default option
                    const defaultOption = document.createElement('option');
                    defaultOption.value = '';
                    defaultOption.text = 'Chọn quy định';
                    selectElement.appendChild(defaultOption);

                    // Add available regulations
                    availableChoices.forEach(reg => {
                        const option = document.createElement('option');
                        option.value = reg.id;
                        option.text = `${reg.name} (${reg.effectiveStartDate} - ${reg.effectiveEndDate})`;
                        selectElement.appendChild(option);
                    });

                    // Restore current selection if it exists
                    if (currentValue) {
                        selectElement.value = currentValue;
                    }
                }
            });
        },

        /**
         * Set up form validation
         */
        setupValidation() {
            const self = this; // Store reference to component

            // Add custom validation methods

            $.validator.addMethod("customPattern", function (value, element, regex) {
                return this.optional(element) || regex.test(value);
            }, "Giá trị không hợp lệ.");

            $.validator.addMethod("numberWithComma", function (value, element) {
                var cleanValue = value.replace(/,/g, '');
                return this.optional(element) || !isNaN(cleanValue) && cleanValue >= 0;
            }, "Tiền ứng trước phải là số.");

            $.validator.addMethod("minWithComma", function (value, element, param) {
                var cleanValue = value.replace(/,/g, '');
                return this.optional(element) || !isNaN(cleanValue) && Number(cleanValue) >= param;
            }, "Tiền ứng trước phải lớn hơn 0.");

            $("#createForm").validate({
                ignore: [],
                rules: {
                    "Diagnosis": { required: true },
                    "StartDate": { required: true, dateFormat: true, notPastDate: true },
                    "EndDate": { required: true, dateFormat: true, endDateAfterStartDate: true },
                    "PatientId": { required: true },
                    "TreatmentRecordDetail.TreatmentMethodId": { required: true },
                    "TreatmentRecordDetail.RoomId": { required: true },
                    "Assignment.StartDate": {
                        required: true,
                        dateFormat: true,
                        notPastDate: true,
                        assignmentStartDateValid: true
                    },
                    "Assignment.EndDate": {
                        required: true,
                        dateFormat: true,
                        endDateAfterStartDate: true,
                        assignmentEndDateValid: true
                    }
                },
                messages: {
                    "Diagnosis": { required: "Chẩn đoán không được bỏ trống." },
                    "StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống.",
                        dateFormat: "Ngày bắt đầu không hợp lệ."
                    },
                    "EndDate": {
                        required: "Ngày kết thúc không được bỏ trống.",
                        dateFormat: "Ngày kết thúc không hợp lệ."
                    },
                    "PatientId": { required: "Bệnh nhân không được bỏ trống." },
                    "TreatmentRecordDetail.TreatmentMethodId": { required: "Phương pháp điều trị không được bỏ trống." },
                    "TreatmentRecordDetail.RoomId": { required: "Phòng điều trị không được bỏ trống." },
                    "Assignment.StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống.",
                        dateFormat: "Ngày bắt đầu không hợp lệ.",
                        notPastDate: "Ngày bắt đầu không thể là ngày trong quá khứ.",
                        assignmentStartDateValid: "Ngày bắt đầu phân công không được trước ngày bắt đầu điều trị"
                    },
                    "Assignment.EndDate": {
                        required: "Ngày kết thúc không được bỏ trống.",
                        dateFormat: "Ngày kết thúc không hợp lệ.",
                        endDateAfterStartDate: "Ngày kết thúc phải sau ngày bắt đầu.",
                        assignmentEndDateValid: "Ngày kết thúc phân công không được sau ngày kết thúc điều trị"
                    }
                },
                errorElement: "div",
                errorClass: "text-danger",
                highlight: function (element) {
                    // Xử lý cho select2, choices, regulation-date, regulation-select, ...
                    if ($(element).is('select')) {
                        const choicesContainer = $(element).closest('.select-wrapper').find('.choices__inner');
                        if (choicesContainer.length) {
                            choicesContainer.addClass("border-red-500");
                        } else {
                            $(element).addClass("border-red-500");
                        }
                    } else {
                        $(element).addClass("border-red-500");
                    }
                    // Bổ sung cho các trường động
                    if ($(element).hasClass('regulation-date') || $(element).hasClass('regulation-select')) {
                        $(element).addClass('border-red-500');
                    }
                    // Bổ sung cho các select phòng động
                    if ($(element).attr('name') && $(element).attr('name').includes('TreatmentRecordDetails') && $(element).is('select')) {
                        $(element).addClass('border-red-500');
                    }
                },
                unhighlight: function (element) {
                    if ($(element).is('select')) {
                        const choicesContainer = $(element).closest('.select-wrapper').find('.choices__inner');
                        if (choicesContainer.length) {
                            choicesContainer.removeClass("border-red-500");
                        } else {
                            $(element).removeClass("border-red-500");
                        }
                    } else {
                        $(element).removeClass("border-red-500");
                    }
                    // Bổ sung cho các trường động
                    if ($(element).hasClass('regulation-date') || $(element).hasClass('regulation-select')) {
                        $(element).removeClass('border-red-500');
                    }
                    // Bổ sung cho các select phòng động
                    if ($(element).attr('name') && $(element).attr('name').includes('TreatmentRecordDetails') && $(element).is('select')) {
                        $(element).removeClass('border-red-500');
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
            // Add validation for select elements
            $('select.form-input').on('change', function () {
                $(this).valid();
            });
            // Bổ sung validate cho các trường động
            $(document).on('change', '.regulation-date, .regulation-select', function () {
                $(this).valid();
            });
            $(document).on('change', 'select[name^="TreatmentRecordDetails"][name$=".RoomId"]', function () {
                $(this).valid();
            });
        },

        /**
         * Submit form handler
         */
        submitForm() {
            // Kiểm tra validate client trước khi submit
            if (!$('#createForm').valid()) {
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
            // Log all form data before sending
            const formData = new FormData(document.getElementById('createForm'));
            this.appendFormData(formData);
            const overlay = document.getElementById('loadingOverlay');
            overlay.style.display = 'flex';
            try {
                // Nếu không có file, submit form bình thường
                fetch('/phieu-dieu-tri/lap-phieu', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        return response.json();
                    })
                    .then(this.handleResponse)
                    .catch(error => {
                        overlay.style.display = 'none';
                        notyf.error("Có lỗi xảy ra khi gửi yêu cầu: " + error.message);
                    });
            } catch (error) {
                overlay.style.display = 'none';
                notyf.error("Có lỗi xảy ra: " + error.message);
            }
        },

        /**
         * Handle form submission response
         */
        handleResponse(response) {
            const overlay = document.getElementById('loadingOverlay');
            if (response.success) {
                overlay.style.display = 'flex';
                notyf.success(response.message);
                setTimeout(() => {
                    window.location.href = '/phieu-dieu-tri';
                }, 2000);
            } else {
                overlay.style.display = 'none';
                notyf.error(response.message);
                if (response.errors) {
                    response.errors.forEach(error => notyf.error(error));
                }
            }
        },

        /**
         * Set up listeners for treatment date changes
         */
        setupTreatmentDateListeners() {
            const startDateInput = document.getElementById('StartDate');
            const endDateInput = document.getElementById('EndDate');
            const warningDiv = document.getElementById('regulationDateWarning');

            const updateRegulationState = () => {
                const hasStartDate = startDateInput.value.trim() !== '';
                const hasEndDate = endDateInput.value.trim() !== '';
                const regulationSelects = document.querySelectorAll('[id^="regulationId-"]');

                if (!hasStartDate || !hasEndDate) {
                    // Disable all regulation selects
                    regulationSelects.forEach(select => {
                        select.disabled = true;
                        select.classList.add('opacity-50', 'cursor-not-allowed');
                    });
                    if (warningDiv) warningDiv.style.display = 'block';
                } else {
                    // Enable all regulation selects
                    regulationSelects.forEach(select => {
                        select.disabled = false;
                        select.classList.remove('opacity-50', 'cursor-not-allowed');
                    });
                    if (warningDiv) warningDiv.style.display = 'none';
                }
            };

            if (startDateInput) {
                startDateInput.addEventListener('change', updateRegulationState);
            }
            if (endDateInput) {
                endDateInput.addEventListener('change', updateRegulationState);
            }

            // Initial state check
            updateRegulationState();
        },

        goBack() {
            window.location.href = '/phieu-dieu-tri';
        }
    }));

    Alpine.data('treatmentDetailTable', () => ({
        details: [],
        treatmentMethods: window.treatmentMethodsData || [],
        allRooms: window.roomsData || [],
        addDetail() {
            this.details.push({
                Code: this.generateCode(),
                TreatmentMethodId: '',
                RoomId: '',
                Note: ''
            });
            this.$nextTick(() => {
                this.updateRoomCursor(this.details.length - 1);
            });
        },
        removeDetail(idx) {
            this.details.splice(idx, 1);
            this.$nextTick(() => {
                this.details.forEach((_, i) => this.updateRoomCursor(i));
            });
        },
        onMethodChange(idx) {
            this.details[idx].RoomId = '';
            this.$nextTick(() => {
                this.updateRoomCursor(idx);
            });
        },
        getRoomsForMethod(methodId) {
            if (!methodId) return [];
            return this.allRooms.filter(r => r.treatmentMethodId == methodId);
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
            const selectedIds = this.details
                .map((d, idx) => idx !== currentIdx ? d.TreatmentMethodId : null)
                .filter(id => id);
            return this.treatmentMethods.filter(m => !selectedIds.includes(m.id));
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

// Custom jQuery validation methods
$.validator.addMethod("dateFormat", function (value, element) {
    return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
}, "Vui lòng nhập ngày hợp lệ (dd/mm/yyyy).");


$.validator.addMethod("endDateAfterStartDate", function (value, element) {
    const startDate = $("#StartDate").val();
    if (!startDate || !value) return true;

    const start = new Date(startDate.split('/').reverse().join('-'));
    const end = new Date(value.split('/').reverse().join('-'));
    return end > start;
}, "Ngày kết thúc phải sau ngày bắt đầu.");

$.validator.addMethod("notPastDate", function (value, element) {
    if (!value) return true;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const selectedDate = new Date(value.split('/').reverse().join('-'));
    selectedDate.setHours(0, 0, 0, 0);

    return selectedDate >= today;
}, "Không thể chọn ngày trong quá khứ.");

$.validator.addMethod("assignmentStartDateValid", function (value, element) {
    if (!value) return true;

    const treatmentStartDate = $("#StartDate").val();
    if (!treatmentStartDate) return true;

    const assignmentStart = new Date(value.split('/').reverse().join('-'));
    const treatmentStart = new Date(treatmentStartDate.split('/').reverse().join('-'));

    return assignmentStart >= treatmentStart;
}, "Ngày bắt đầu phân công không được trước ngày bắt đầu điều trị");

$.validator.addMethod("assignmentEndDateValid", function (value, element) {
    if (!value) return true;

    const treatmentEndDate = $("#EndDate").val();
    if (!treatmentEndDate) return true;

    const assignmentEnd = new Date(value.split('/').reverse().join('-'));
    const treatmentEnd = new Date(treatmentEndDate.split('/').reverse().join('-'));

    return assignmentEnd <= treatmentEnd;
}, "Ngày kết thúc phân công không được sau ngày kết thúc điều trị");

function checkTreatmentMethod() {
    const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');
    const roomSelect = document.getElementById('treatmentRecordDetailRoom');
    const warningDiv = document.getElementById('treatmentMethodWarning');

    if (!treatmentMethodSelect || !roomSelect || !warningDiv) return;

    if (!treatmentMethodSelect.value) {
        roomSelect.disabled = true;
        warningDiv.style.display = 'block';
    } else {
        roomSelect.disabled = false;
        warningDiv.style.display = 'none';
    }
}