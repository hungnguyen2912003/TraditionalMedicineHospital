// Disable auto-discovery for Dropzone
Dropzone.autoDiscover = false;

/**
 * Reception module for handling patient reception functionality
 */
document.addEventListener('alpine:init', () => {
    Alpine.data('reception', () => ({
        // State
        dropzone: null,
        filteredRooms: [],
        allRooms: [],
        roomChoices: null,
        regulations: [],
        availableRegulations: [],
        hasHealthInsurance: true,
        images: [],
        errors: {},
        choicesInstances: {}, // Store Choices instances
        assignmentStartPicker: null,
        assignmentEndPicker: null,

        /**
         * Initialize the reception component
         */
        init() {
            try {
                // Initialize arrays first
                this.filteredRooms = [];
                this.allRooms = window.roomsData || [];
                this.availableRegulations = window.regulationsData || [];

                // Then initialize components once
                this.initializeComponents();
            } catch (error) {
                console.error('Error initializing reception:', error);
                notyf.error('Có lỗi xảy ra khi khởi tạo form');
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

                // Parse regulation effective dates
                const regStart = this.parseDate(r.effectiveStartDate);
                const regEnd = this.parseDate(r.effectiveEndDate);

                // Check if regulation's effective period overlaps with treatment period
                // and regulation starts before treatment ends
                return regStart <= treatmentEnd && regEnd >= treatmentStart && regStart <= treatmentEnd;
            });
        },

        /**
         * Add a new regulation
         */
        addRegulation() {
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

            // Initialize Choices.js for the new regulation select after the template is rendered
            this.$nextTick(() => {
                const index = this.regulations.length - 1;
                const selectElement = document.getElementById(`regulationId-${index}`);
                if (selectElement) {
                    new Choices(selectElement, {
                        removeItemButton: true,
                        searchEnabled: true,
                        placeholder: true,
                        placeholderValue: 'Chọn quy định',
                        noResultsText: 'Không tìm thấy kết quả',
                        itemSelectText: ''
                    });
                }
            });
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

        /**
         * Initialize all form components
         */
        initializeComponents() {
            this.setupDropzone();
            this.setupValidation();
            this.setupDateTimePicker();
            this.setupChoices();
        },

        /**
         * Set up Dropzone for image uploads
         */
        setupDropzone() {
            const dropzoneElement = document.getElementById('imageDropzone');
            if (!dropzoneElement || dropzoneElement.dropzone) return;

            const self = this;
            this.dropzone = new Dropzone('#imageDropzone', {
                url: '/Staff/Receptions/Create',
                autoProcessQueue: false,
                maxFiles: 1,
                acceptedFiles: 'image/*',
                addRemoveLinks: true,
                dictDefaultMessage: 'Kéo thả hoặc nhấp để chọn ảnh',
                paramName: 'Patient.ImageFile',
                init: function () {
                    this.on('addedfile', (file) => {
                        if (this.files.length > 1) {
                            this.removeFile(this.files[0]);
                        }
                    });

                    this.on('sending', (file, xhr, formData) => {
                        self.appendFormData(formData);
                    });

                    this.on('success', (file, response) => {
                        self.handleResponse(response);
                    });

                    this.on('error', (file, errorMessage) => {
                        const overlay = document.getElementById('loadingOverlay');
                        overlay.style.display = 'none';
                        notyf.error("Có lỗi xảy ra: " + errorMessage);
                    });
                }
            });
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
            formData.append('Patient.Gender', document.getElementById('Gender').value);
            formData.append('Patient.DateOfBirth', document.getElementById('DateOfBirth').value);
            formData.append('Patient.IdentityNumber', document.getElementById('IdentityNumber').value);
            formData.append('Patient.Address', document.getElementById('Address').value);
            formData.append('Patient.PhoneNumber', document.getElementById('PhoneNumber').value);
            formData.append('Patient.EmailAddress', document.getElementById('Email').value);
            formData.append('Patient.HasHealthInsurance', document.getElementById('HasHealthInsurance').checked);

            // Append health insurance data if exists
            if (document.getElementById('HasHealthInsurance').checked) {
                formData.append('Patient.HealthInsuranceCode', document.getElementById('HealthInsuranceCode').value);
                formData.append('Patient.HealthInsuranceNumber', document.getElementById('HealthInsuranceNumber').value);
                formData.append('Patient.HealthInsuranceExpiryDate', document.getElementById('HealthInsuranceExpiryDate').value);
                formData.append('Patient.HealthInsurancePlaceOfRegistration', document.getElementById('HealthInsurancePlaceOfRegistration').value);
            }

            // Append treatment record detail data
            formData.append('TreatmentRecordDetail.Code', document.getElementById('treatmentRecordDetailCode').value);
            formData.append('TreatmentRecordDetail.TreatmentMethodId', document.getElementById('treatmentRecordDetailTreatmentMethod').value);
            formData.append('TreatmentRecordDetail.RoomId', document.getElementById('treatmentRecordDetailRoom').value);
            formData.append('TreatmentRecordDetail.Note', document.getElementById('treatmentRecordDetailNote').value);

            // Append assignment data
            formData.append('Assignment.Code', document.getElementById('assignmentCode').value);
            formData.append('Assignment.StartDate', document.getElementById('assignmentStartDate').value);
            formData.append('Assignment.EndDate', document.getElementById('assignmentEndDate').value);
            formData.append('Assignment.Note', document.getElementById('assignmentNote').value);

            // Append regulations data
            const regulationElements = document.querySelectorAll('[id^="regulationId-"]');
            regulationElements.forEach((element, index) => {
                const regulationId = element.value;
                const executionDate = document.getElementById(`executionDate-${index}`).value;
                const note = document.getElementById(`note-${index}`).value;
                const code = document.querySelector(`[name="Regulations[${index}].Code"]`).value;

                if (regulationId) {
                    formData.append(`Regulations[${index}].Code`, code);
                    formData.append(`Regulations[${index}].RegulationId`, regulationId);
                    formData.append(`Regulations[${index}].ExecutionDate`, executionDate);
                    formData.append(`Regulations[${index}].Note`, note);
                }
            });

            // Add antiforgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            formData.append('__RequestVerificationToken', token);
        },

        /**
         * Set up Choices.js for select elements
         */
        setupChoices() {
            const selectElements = document.querySelectorAll('select.choices');
            selectElements.forEach(select => {
                // Skip if already initialized or is room select
                if (select.id === 'treatmentRecordDetailRoom' || select.choices) return;

                const choices = new Choices(select, {
                    searchEnabled: true,
                    searchPlaceholderValue: 'Tìm kiếm...',
                    removeItemButton: true,
                    noResultsText: 'Không tìm thấy kết quả',
                    noChoicesText: 'Không có lựa chọn nào',
                    itemSelectText: ''
                });

                // Store the instance
                this.choicesInstances[select.id] = choices;
                select.choices = choices; // Mark as initialized
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

            // Create a hidden input for the room ID if it doesn't exist
            let hiddenInput = document.querySelector('input[name="TreatmentRecordDetail.RoomId"]');
            if (!hiddenInput) {
                hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = 'TreatmentRecordDetail.RoomId';
                roomSelect.parentNode.appendChild(hiddenInput);
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
                        hiddenInput.value = selectedValue;
                        console.log('Room selected:', selectedValue);
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

                // Setup assignment start date picker
                if (assignmentStartInput) {
                    this.assignmentStartPicker = flatpickr(assignmentStartInput, {
                        dateFormat: "d/m/Y",
                        allowInput: true,
                        onChange: (selectedDates, dateStr) => {
                            if (this.assignmentEndPicker && selectedDates[0]) {
                                this.assignmentEndPicker.set('minDate', selectedDates[0]);
                            }
                        },
                        onOpen: (selectedDates, dateStr, instance) => {
                            if (!areTreatmentDatesSelected()) {
                                instance.close();
                                return;
                            }
                            instance.set('minDate', treatmentStartDate.value);
                            instance.set('maxDate', treatmentEndDate.value);
                        }
                    });
                }

                // Setup assignment end date picker
                if (assignmentEndInput) {
                    this.assignmentEndPicker = flatpickr(assignmentEndInput, {
                        dateFormat: "d/m/Y",
                        allowInput: true,
                        onOpen: (selectedDates, dateStr, instance) => {
                            if (!areTreatmentDatesSelected()) {
                                instance.close();
                                return;
                            }
                            const minDate = assignmentStartInput.value || treatmentStartDate.value;
                            instance.set('minDate', minDate);
                            instance.set('maxDate', treatmentEndDate.value);
                        }
                    });
                }

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
                            if (this.assignmentStartPicker) this.assignmentStartPicker.set('maxDate', this.value);
                            if (this.assignmentEndPicker) this.assignmentEndPicker.set('maxDate', this.value);
                        }
                    });
                }

                // Initial state setup
                updateAssignmentDatePickersState();

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
                console.error('Error setting up date time picker:', error);
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

            flatpickr(element, {
                dateFormat: "d/m/Y",
                allowInput: true,
                minDate: startDate,
                maxDate: endDate,
                disable: [
                    function (date) {
                        // Disable dates outside the effective range
                        return date < startDate || date > endDate;
                    }
                ]
            });
        },

        /**
         * Parse date string (dd/mm/yyyy) to Date object
         */
        parseDate(dateStr) {
            const [day, month, year] = dateStr.split('/').map(Number);
            return new Date(year, month - 1, day);
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
                } else {
                    // Clear execution date if regulation is deselected
                    this.regulations[index].ExecutionDate = '';
                }
            }
        },

        /**
         * Check if person is over 14 years old
         */
        isOver14(dateOfBirth) {
            if (!dateOfBirth) return false;

            const parts = dateOfBirth.split('/');
            const birthDate = new Date(parts[2], parts[1] - 1, parts[0]);
            const today = new Date();
            const age = today.getFullYear() - birthDate.getFullYear();
            const monthDiff = today.getMonth() - birthDate.getMonth();

            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
                return age - 1 >= 14;
            }

            return age >= 14;
        },

        /**
         * Set up form validation
         */
        setupValidation() {
            const self = this;
            const validationRules = this.getValidationRules();
            const validationMessages = this.getValidationMessages();

            // Add custom validation method for CCCD based on age
            $.validator.addMethod("requiredIfOver14", function (value, element) {
                const dateOfBirth = $("#DateOfBirth").val();
                if (!dateOfBirth) return true; // Skip validation if DOB not entered

                const isOver14 = self.isOver14(dateOfBirth);
                if (isOver14) {
                    return value.trim().length > 0;
                }
                return true;
            }, "Người trên 14 tuổi bắt buộc phải nhập CCCD");

            // Add custom validation method for health insurance expiry date
            $.validator.addMethod("notExpired", function (value, element) {
                if (!value) return true; // Skip validation if no date entered

                const parts = value.split('/');
                const expiryDate = new Date(parts[2], parts[1] - 1, parts[0]);
                const today = new Date();
                today.setHours(0, 0, 0, 0);

                return expiryDate >= today;
            }, "Thẻ BHYT đã hết hạn");

            $("#receptionForm").validate({
                ignore: [],
                rules: validationRules,
                messages: validationMessages,
                errorElement: "div",
                errorClass: "text-danger",
                highlight: this.highlightError,
                unhighlight: this.unhighlightError,
                errorPlacement: this.placeError,
                onfocusout: this.validateOnFocusOut,
                onkeyup: false
            });

            // Add validation for select elements
            $('select.form-input').on('change', function () {
                $(this).valid();
            });

            // Add validation when date of birth changes
            $("#DateOfBirth").on('change', function () {
                $("#IdentityNumber").valid();
            });
        },

        /**
         * Get validation rules for the form
         */
        getValidationRules() {
            return {
                "Name": { required: true, minlength: 2, maxlength: 50 },
                "DateOfBirth": { required: true, dateFormat: true },
                "Gender": { required: true },
                "IdentityNumber": {
                    requiredIfOver14: true,
                    minlength: 9,
                    maxlength: 12,
                    remote: {
                        url: "/api/validation/patient/check",
                        type: "GET",
                        data: {
                            type: "identitynumber",
                            entityType: "patient",
                            value: function () { return $("#IdentityNumber").val(); }
                        },
                        dataFilter: function (data) {
                            return JSON.parse(data) === true;
                        }
                    }
                },
                "PhoneNumber": {
                    required: true,
                    phone: true,
                    remote: {
                        url: "/api/validation/patient/check",
                        type: "GET",
                        data: {
                            type: "phone",
                            entityType: "patient",
                            value: function () { return $("#PhoneNumber").val(); }
                        },
                        dataFilter: function (data) {
                            return JSON.parse(data) === true;
                        }
                    }
                },
                "Address": { required: true, minlength: 5, maxlength: 500 },
                "Email": { email: true },
                "HealthInsuranceNumber": {
                    required: () => $('#HasHealthInsurance').is(':checked'),
                    remote: {
                        url: "/api/validation/healthinsurance/check",
                        type: "GET",
                        data: {
                            type: "numberhealthinsurance",
                            entityType: "healthinsurance",
                            value: function () { return $("#HealthInsuranceNumber").val(); }
                        },
                        dataFilter: function (data) {
                            return JSON.parse(data) === true;
                        }
                    },
                    minlength: 15,
                    maxlength: 15
                },
                "HealthInsuranceExpiryDate": {
                    required: () => $('#HasHealthInsurance').is(':checked'),
                    dateFormat: true,
                    notExpired: true
                },
                "HealthInsurancePlaceOfRegistration": {
                    required: () => $('#HasHealthInsurance').is(':checked')
                },
                "Diagnosis": { required: true },
                "StartDate": { required: true, dateFormat: true, notPastDate: true },
                "EndDate": { required: true, dateFormat: true, endDateAfterStartDate: true },
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
            };
        },

        /**
         * Get validation messages for the form
         */
        getValidationMessages() {
            return {
                "Name": {
                    required: "Họ và tên không được bỏ trống.",
                    minlength: "Họ và tên phải có ít nhất 2 ký tự.",
                    maxlength: "Họ và tên không được vượt quá 50 ký tự."
                },
                "DateOfBirth": {
                    required: "Ngày sinh không được bỏ trống.",
                    dateFormat: "Ngày sinh không hợp lệ."
                },
                "Gender": { required: "Giới tính không được bỏ trống." },
                "IdentityNumber": {
                    requiredIfOver14: "Người trên 14 tuổi bắt buộc phải nhập CCCD",
                    minlength: "Số CMND/CCCD phải có ít nhất 9 ký tự.",
                    maxlength: "Số CMND/CCCD không được vượt quá 12 ký tự.",
                    remote: "Số CMND/CCCD đã được đăng ký trước đó trên hệ thống"
                },
                "PhoneNumber": {
                    required: "Số điện thoại không được bỏ trống.",
                    phone: "Số điện thoại không hợp lệ.",
                    remote: "Số điện thoại đã được đăng ký trước đó trên hệ thống"
                },
                "Address": {
                    required: "Địa chỉ không được bỏ trống.",
                    minlength: "Địa chỉ phải có ít nhất 5 ký tự.",
                    maxlength: "Địa chỉ không được vượt quá 500 ký tự."
                },
                "Email": { email: "Email không hợp lệ." },
                "HealthInsuranceNumber": { 
                    required: "Số thẻ BHYT không được bỏ trống.",
                    remote: "Số thẻ BHYT đã được đăng ký trước đó trên hệ thống",
                    minlength: "Số thẻ BHYT phải có đủ 15 ký tự.",
                    maxlength: "Số thẻ BHYT phải có đủ 15 ký tự."
                },
                "HealthInsuranceExpiryDate": {
                    required: "Ngày hết hạn không được bỏ trống.",
                    dateFormat: "Ngày hết hạn không hợp lệ.",
                    notExpired: "Thẻ BHYT đã hết hạn"
                },
                "HealthInsurancePlaceOfRegistration": { required: "Nơi đăng ký không được bỏ trống." },
                "Diagnosis": { required: "Chẩn đoán không được bỏ trống." },
                "StartDate": {
                    required: "Ngày bắt đầu không được bỏ trống.",
                    dateFormat: "Ngày bắt đầu không hợp lệ."
                },
                "EndDate": {
                    required: "Ngày kết thúc không được bỏ trống.",
                    dateFormat: "Ngày kết thúc không hợp lệ."
                },
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
            };
        },

        /**
         * Highlight validation errors
         */
        highlightError(element) {
            if ($(element).is('select')) {
                const choicesContainer = $(element).closest('.select-wrapper').find('.choices__inner');
                choicesContainer.addClass("border-red-500");
            } else {
                $(element).addClass("border-red-500");
            }
        },

        /**
         * Remove highlight from valid fields
         */
        unhighlightError(element) {
            if ($(element).is('select')) {
                const choicesContainer = $(element).closest('.select-wrapper').find('.choices__inner');
                choicesContainer.removeClass("border-red-500");
            } else {
                $(element).removeClass("border-red-500");
            }
        },

        /**
         * Place error messages
         */
        placeError(error, element) {
            if (element.is('select')) {
                const wrapper = element.closest('.select-wrapper');
                error.insertAfter(wrapper.length ? wrapper : element);
            } else {
                error.insertAfter(element);
            }
        },

        /**
         * Validate on focus out
         */
        validateOnFocusOut(element) {
            if ($(element).val() === '' || $(element).val().length > 0) {
                $(element).valid();
            }
        },

        /**
         * Filter rooms based on treatment method
         */
        filterRooms(treatmentMethodId) {
            console.log('Treatment Method ID:', treatmentMethodId);
            console.log('All Rooms:', this.allRooms);

            const roomSelect = document.getElementById('treatmentRecordDetailRoom');
            const warningDiv = document.getElementById('treatmentMethodWarning');
            const hiddenInput = document.querySelector('input[name="TreatmentRecordDetail.RoomId"]');

            if (!treatmentMethodId) {
                if (this.roomChoices) {
                    this.roomChoices.clearStore();
                    this.roomChoices.setChoices([{ value: '', label: 'Chọn phòng' }], 'value', 'label', true);
                    this.roomChoices.disable();
                }
                if (hiddenInput) hiddenInput.value = '';
                warningDiv.style.display = 'block';
                return;
            }

            warningDiv.style.display = 'none';

            // Filter rooms
            const filteredRooms = this.allRooms.filter(room => {
                const roomMethodId = String(room.treatmentMethodId || '').toLowerCase().trim();
                const selectedMethodId = String(treatmentMethodId).toLowerCase().trim();
                return roomMethodId === selectedMethodId;
            });

            console.log('Filtered Rooms:', filteredRooms);

            // Update choices
            if (this.roomChoices) {
                this.roomChoices.clearStore();

                const choices = [
                    { value: '', label: 'Chọn phòng' },
                    ...filteredRooms.map(room => ({
                        value: room.id,
                        label: room.name
                    }))
                ];

                this.roomChoices.setChoices(choices, 'value', 'label', true);

                if (filteredRooms.length > 0) {
                    this.roomChoices.enable();
                } else {
                    this.roomChoices.disable();
                    if (hiddenInput) hiddenInput.value = '';
                }
            }
        },

        /**
         * Submit the form
         */
        submitForm() {
            const form = document.getElementById('receptionForm');
            if (!$("#receptionForm").valid()) {
                notyf.error("Vui lòng kiểm tra lại thông tin đã nhập.");
                return;
            }

            const overlay = document.getElementById('loadingOverlay');
            overlay.style.display = 'flex';

            try {
                if (this.dropzone?.files.length > 0 && this.dropzone.getQueuedFiles().length > 0) {
                    this.dropzone.processQueue();
                } else {
                    const formData = new FormData(form);
                    this.appendFormData(formData);

                    fetch('/Staff/Receptions/Create', {
                        method: 'POST',
                        body: formData
                    })
                        .then(response => response.json())
                        .then(this.handleResponse)
                        .catch(error => {
                            overlay.style.display = 'none';
                            console.error('Error submitting form:', error);
                            notyf.error("Có lỗi xảy ra khi gửi yêu cầu: " + error.message);
                        });
                }
            } catch (error) {
                overlay.style.display = 'none';
                console.error('Error in submitForm:', error);
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
                    window.location.href = '/Staff/';
                }, 2000);
            } else {
                overlay.style.display = 'none';
                notyf.error(response.message);
                if (response.errors) {
                    response.errors.forEach(error => notyf.error(error));
                }
            }
        },

        // Handle file upload
        handleFileUpload(e) {
            const files = Array.from(e.target.files);
            this.images = [...this.images, ...files];
        },

        // Remove uploaded file
        removeFile(index) {
            this.images.splice(index, 1);
        },

        // Email validation
        validateEmail(email) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(email);
        }
    }));
});

// Custom jQuery validation methods
$.validator.addMethod("dateFormat", function (value, element) {
    return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
}, "Vui lòng nhập ngày hợp lệ (dd/mm/yyyy).");

$.validator.addMethod("phone", function (value, element) {
    return this.optional(element) || /^\+?\d{10,12}$/.test(value);
}, "Số điện thoại không hợp lệ.");

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

    if (!treatmentMethodSelect.value) {
        roomSelect.disabled = true;
        warningDiv.style.display = 'block';
    } else {
        roomSelect.disabled = false;
        warningDiv.style.display = 'none';
    }
}

// Call this when the page loads to set initial state
document.addEventListener('DOMContentLoaded', function () {
    checkTreatmentMethod();
});