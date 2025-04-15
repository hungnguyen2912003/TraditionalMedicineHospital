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
        
        /**
         * Initialize the reception component
         */
        init() {
            try {
                this.allRooms = window.roomsData || [];
                this.initializeComponents();
            } catch (error) {
                console.error('Error initializing reception:', error);
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
            if (!dropzoneElement) return;

            const dropzoneConfig = {
                url: '/Staff/Receptions/Create',
                autoProcessQueue: false,
                maxFiles: 1,
                acceptedFiles: 'image/*',
                addRemoveLinks: true,
                dictDefaultMessage: 'Kéo thả hoặc nhấp để chọn ảnh',
                paramName: 'file',
                init: () => this.initializeDropzoneEvents()
            };

            try {
                this.dropzone = new Dropzone(dropzoneElement, dropzoneConfig);
            } catch (error) {
                console.error('Error initializing Dropzone:', error);
                notyf.error('Có lỗi xảy ra khi khởi tạo vùng upload ảnh');
            }
        },

        /**
         * Initialize Dropzone events
         */
        initializeDropzoneEvents() {
            return {
                addedfile: (file) => {
                    if (this.dropzone.files.length > 1) {
                        this.dropzone.removeFile(this.dropzone.files[0]);
                    }
                },
                removedfile: () => {
                    const imageInput = document.querySelector('input[name="Images"]');
                    imageInput?.remove();
                },
                success: (file, response) => {
                    if (response.success) {
                        this.handleUploadSuccess(response.fileName);
                    } else {
                        this.handleUploadError(file, response.message);
                    }
                },
                error: (file, errorMessage) => {
                    this.handleUploadError(file, errorMessage);
                }
            };
        },

        /**
         * Handle successful file upload
         */
        handleUploadSuccess(fileName) {
            const imagePathInput = document.createElement('input');
            imagePathInput.type = 'hidden';
            imagePathInput.name = 'Images';
            imagePathInput.value = fileName;
            document.getElementById('receptionForm')?.appendChild(imagePathInput);
        },

        /**
         * Handle file upload error
         */
        handleUploadError(file, message) {
            this.dropzone.removeFile(file);
            notyf.error(message || 'Có lỗi xảy ra khi upload ảnh');
        },

        /**
         * Set up Choices.js for select elements
         */
        setupChoices() {
            const selectElements = document.querySelectorAll('select.choices');
            selectElements.forEach(select => {
                if (select.id !== 'treatmentRecordDetailRoom') {
                    new Choices(select, {
                        searchEnabled: true,
                        searchPlaceholderValue: 'Tìm kiếm...',
                        removeItemButton: true,
                        noResultsText: 'Không tìm thấy kết quả',
                        noChoicesText: 'Không có lựa chọn nào',
                        itemSelectText: ''
                    });
                }
            });

            this.initRoomChoices();
        },

        /**
         * Initialize Choices.js for room select
         */
        initRoomChoices() {
            const roomSelect = document.getElementById('treatmentRecordDetailRoom');
            if (!roomSelect) return;

            if (this.roomChoices) {
                this.roomChoices.destroy();
            }

            this.roomChoices = new Choices(roomSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm phòng...',
                removeItemButton: true,
                noResultsText: 'Không tìm thấy phòng',
                noChoicesText: 'Không có phòng nào',
                itemSelectText: ''
            });
        },

        /**
         * Set up Flatpickr for date inputs
         */
        setupDateTimePicker() {
            // Regular date pickers
            flatpickr('.flatpickr:not(#assignmentStartDate):not(#assignmentEndDate)', {
                dateFormat: "d/m/Y",
                allowInput: true,
                onChange: (selectedDates, dateStr, instance) => {
                    // If this is the treatment start date, update assignment start date
                    if (instance.element.id === 'StartDate') {
                        const assignmentStartDate = document.getElementById('assignmentStartDate');
                        if (assignmentStartDate) {
                            assignmentStartDate.value = dateStr;
                            if (assignmentStartPicker) {
                                assignmentStartPicker.setDate(dateStr);
                            }
                        }
                    }
                    // If this is the treatment end date, update assignment end date
                    if (instance.element.id === 'EndDate') {
                        const assignmentEndDate = document.getElementById('assignmentEndDate');
                        if (assignmentEndDate) {
                            assignmentEndDate.value = dateStr;
                            if (assignmentEndPicker) {
                                assignmentEndPicker.setDate(dateStr);
                            }
                        }
                    }
                }
            });

            // Assignment date pickers with constraints
            const treatmentStartDate = document.getElementById('StartDate');
            const treatmentEndDate = document.getElementById('EndDate');

            // Setup assignment start date picker
            const assignmentStartPicker = flatpickr('#assignmentStartDate', {
                dateFormat: "d/m/Y",
                allowInput: true,
                onChange: function(selectedDates, dateStr) {
                    // Update the minimum date of the assignment end date picker
                    if (assignmentEndPicker && selectedDates[0]) {
                        assignmentEndPicker.set('minDate', selectedDates[0]);
                    }
                }
            });

            // Setup assignment end date picker
            const assignmentEndPicker = flatpickr('#assignmentEndDate', {
                dateFormat: "d/m/Y",
                allowInput: true
            });

            // Set initial constraints and values
            const initializeDates = () => {
                const startDate = treatmentStartDate.value;
                const endDate = treatmentEndDate.value;

                if (startDate) {
                    assignmentStartPicker.set('minDate', startDate);
                    document.getElementById('assignmentStartDate').value = startDate;
                    assignmentStartPicker.setDate(startDate);
                }

                if (endDate) {
                    assignmentStartPicker.set('maxDate', endDate);
                    assignmentEndPicker.set('maxDate', endDate);
                    document.getElementById('assignmentEndDate').value = endDate;
                    assignmentEndPicker.setDate(endDate);
                }
            };

            // Initialize dates on page load
            initializeDates();

            // Update constraints when treatment dates change
            treatmentStartDate.addEventListener('change', function() {
                const startDate = this.value;
                if (startDate) {
                    assignmentStartPicker.set('minDate', startDate);
                    document.getElementById('assignmentStartDate').value = startDate;
                    assignmentStartPicker.setDate(startDate);
                }
            });

            treatmentEndDate.addEventListener('change', function() {
                const endDate = this.value;
                if (endDate) {
                    assignmentStartPicker.set('maxDate', endDate);
                    assignmentEndPicker.set('maxDate', endDate);
                    document.getElementById('assignmentEndDate').value = endDate;
                    assignmentEndPicker.setDate(endDate);
                }
            });
        },

        /**
         * Set up form validation
         */
        setupValidation() {
            const validationRules = this.getValidationRules();
            const validationMessages = this.getValidationMessages();

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
            $('select.form-input').on('change', function() {
                $(this).valid();
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
                "IdentityNumber": { required: true, minlength: 9, maxlength: 12 },
                "PhoneNumber": { required: true, phone: true },
                "Address": { required: true, minlength: 5, maxlength: 500 },
                "Email": { email: true },
                "HealthInsuranceNumber": { 
                    required: () => $('#HasHealthInsurance').is(':checked')
                },
                "HealthInsuranceExpiryDate": { 
                    required: () => $('#HasHealthInsurance').is(':checked'),
                    dateFormat: true 
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
                    required: "Số CMND/CCCD không được bỏ trống.",
                    minlength: "Số CMND/CCCD phải có ít nhất 9 ký tự.",
                    maxlength: "Số CMND/CCCD không được vượt quá 12 ký tự."
                },
                "PhoneNumber": {
                    required: "Số điện thoại không được bỏ trống.",
                    phone: "Số điện thoại không hợp lệ."
                },
                "Address": {
                    required: "Địa chỉ không được bỏ trống.",
                    minlength: "Địa chỉ phải có ít nhất 5 ký tự.",
                    maxlength: "Địa chỉ không được vượt quá 500 ký tự."
                },
                "Email": { email: "Email không hợp lệ." },
                "HealthInsuranceNumber": { required: "Số thẻ BHYT không được bỏ trống." },
                "HealthInsuranceExpiryDate": {
                    required: "Ngày hết hạn không được bỏ trống.",
                    dateFormat: "Ngày hết hạn không hợp lệ."
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
            if (!treatmentMethodId) {
                this.filteredRooms = [];
            } else {
                this.filteredRooms = this.allRooms.filter(room => 
                    room.treatmentMethodId === treatmentMethodId
                );
            }

            this.$nextTick(() => {
                this.initRoomChoices();
            });
        },

        /**
         * Submit the form
         */
        async submitForm() {
            if (!$("#receptionForm").valid()) {
                notyf.error("Vui lòng kiểm tra lại thông tin đã nhập.");
                return;
            }

            const overlay = document.getElementById('loadingOverlay');
            overlay.style.display = 'flex';

            try {
                if (this.dropzone?.files.length > 0) {
                    this.dropzone.processQueue();
                }

                const formData = new FormData(document.getElementById('receptionForm'));
                const response = await this.submitFormData(formData);

                if (response.success) {
                    notyf.success(response.message);
                    setTimeout(() => {
                        window.location.href = '/Staff/Receptions/';
                    }, 2000);
                } else {
                    this.handleSubmitError(response);
                }
            } catch (error) {
                console.error('Error submitting form:', error);
                notyf.error("Có lỗi xảy ra khi gửi yêu cầu: " + error.message);
            } finally {
                overlay.style.display = 'none';
            }
        },

        /**
         * Submit form data to the server
         */
        async submitFormData(formData) {
            const response = await fetch('/Staff/Receptions/Create', {
                method: 'POST',
                body: formData
            });
            return await response.json();
        },

        /**
         * Handle form submission error
         */
        handleSubmitError(response) {
            notyf.error(response.message);
            if (response.errors) {
                response.errors.forEach(error => notyf.error(error));
            }
        }
    }));
});

// Custom jQuery validation methods
$.validator.addMethod("dateFormat", function(value, element) {
    return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
}, "Vui lòng nhập ngày hợp lệ (dd/mm/yyyy).");

$.validator.addMethod("phone", function(value, element) {
    return this.optional(element) || /^\+?\d{10,12}$/.test(value);
}, "Số điện thoại không hợp lệ.");

$.validator.addMethod("endDateAfterStartDate", function(value, element) {
    const startDate = $("#StartDate").val();
    if (!startDate || !value) return true;
    
    const start = new Date(startDate.split('/').reverse().join('-'));
    const end = new Date(value.split('/').reverse().join('-'));
    return end > start;
}, "Ngày kết thúc phải sau ngày bắt đầu.");

$.validator.addMethod("notPastDate", function(value, element) {
    if (!value) return true;
    
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    const selectedDate = new Date(value.split('/').reverse().join('-'));
    selectedDate.setHours(0, 0, 0, 0);
    
    return selectedDate >= today;
}, "Không thể chọn ngày trong quá khứ.");

$.validator.addMethod("assignmentStartDateValid", function(value, element) {
    if (!value) return true;
    
    const treatmentStartDate = $("#StartDate").val();
    if (!treatmentStartDate) return true;
    
    const assignmentStart = new Date(value.split('/').reverse().join('-'));
    const treatmentStart = new Date(treatmentStartDate.split('/').reverse().join('-'));
    
    return assignmentStart >= treatmentStart;
}, "Ngày bắt đầu phân công không được trước ngày bắt đầu điều trị");

$.validator.addMethod("assignmentEndDateValid", function(value, element) {
    if (!value) return true;
    
    const treatmentEndDate = $("#EndDate").val();
    if (!treatmentEndDate) return true;
    
    const assignmentEnd = new Date(value.split('/').reverse().join('-'));
    const treatmentEnd = new Date(treatmentEndDate.split('/').reverse().join('-'));
    
    return assignmentEnd <= treatmentEnd;
}, "Ngày kết thúc phân công không được sau ngày kết thúc điều trị");