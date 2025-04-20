// Disable auto-discovery for Dropzone
Dropzone.autoDiscover = false;

/**
 * Reception Edit module for handling patient reception editing functionality
 */
document.addEventListener('alpine:init', () => {
    Alpine.data('receptionEdit', () => ({
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
         * Initialize the reception edit component
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
                console.error('Error initializing reception edit:', error);
                notyf.error('Có lỗi xảy ra khi khởi tạo form');
            }
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
                url: '/Staff/Receptions/Edit',
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

        /**
         * Append form data for submission
         */
        appendFormData(formData) {
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

            // Append new treatment record detail data
            formData.append('NewTreatmentRecordDetail.Code', document.getElementById('treatmentRecordDetailCode').value);
            formData.append('NewTreatmentRecordDetail.TreatmentMethodId', document.getElementById('treatmentRecordDetailTreatmentMethod').value);
            formData.append('NewTreatmentRecordDetail.RoomId', document.getElementById('treatmentRecordDetailRoom').value);
            formData.append('NewTreatmentRecordDetail.Note', document.getElementById('treatmentRecordDetailNote').value);

            // Append new assignment data
            formData.append('NewAssignment.Code', document.getElementById('assignmentCode').value);
            formData.append('NewAssignment.StartDate', document.getElementById('assignmentStartDate').value);
            formData.append('NewAssignment.EndDate', document.getElementById('assignmentEndDate').value);
            formData.append('NewAssignment.Note', document.getElementById('assignmentNote').value);

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
            let hiddenInput = document.querySelector('input[name="NewTreatmentRecordDetail.RoomId"]');
            if (!hiddenInput) {
                hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = 'NewTreatmentRecordDetail.RoomId';
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
         * Generate a unique code for regulations
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
         * Filter rooms based on treatment method
         */
        filterRooms(treatmentMethodId) {
            console.log('Treatment Method ID:', treatmentMethodId);
            console.log('All Rooms:', this.allRooms);

            const roomSelect = document.getElementById('treatmentRecordDetailRoom');
            const warningDiv = document.getElementById('treatmentMethodWarning');
            const hiddenInput = document.querySelector('input[name="NewTreatmentRecordDetail.RoomId"]');

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

                    fetch('/Staff/Receptions/Edit', {
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
        }
    }));
}); 