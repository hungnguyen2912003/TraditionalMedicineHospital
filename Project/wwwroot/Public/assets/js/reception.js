Dropzone.autoDiscover = false;

document.addEventListener('alpine:init', () => {
    Alpine.data('reception', () => ({
        dropzone: null,
        filteredRooms: [],
        allRooms: [],
        uploadedFile: null,

        init() {
            const roomsScript = document.querySelector('script[data-rooms]');
            if (roomsScript) {
                this.allRooms = JSON.parse(roomsScript.textContent);
            }

            this.$nextTick(() => {
                this.setupDateTimePicker();
                this.setupValidation();
                this.setupDropzone();
                this.setupChoices();

                // Listen for tab change event
                window.addEventListener('tab-changed', (event) => {
                    if (event.detail === 'patient') {
                        setTimeout(() => {
                            this.reinitializeDropzone();
                        }, 100);
                    }
                });
            });
        },

        reinitializeDropzone() {
            const dropzoneElement = document.getElementById('imageDropzone');
            if (!dropzoneElement) {
                return;
            }

            if (this.dropzone) {
                this.dropzone.destroy();
                this.dropzone = null;
            }

            try {
                this.dropzone = new Dropzone(dropzoneElement, {
                    url: '/Staff/Receptions/Create',
                    autoProcessQueue: false,
                    maxFiles: 1,
                    acceptedFiles: 'image/*',
                    addRemoveLinks: true,
                    dictDefaultMessage: 'Kéo thả hoặc nhấp để chọn ảnh',
                    paramName: 'file',
                    init: () => {
                        if (this.uploadedFile) {
                            const mockFile = { name: this.uploadedFile.name, size: this.uploadedFile.size };
                            this.dropzone.emit('addedfile', mockFile);
                            this.dropzone.emit('thumbnail', mockFile, this.uploadedFile.dataUrl);
                            this.dropzone.emit('complete', mockFile);
                            this.dropzone.files.push(mockFile);
                        }

                        this.dropzone.on('addedfile', (file) => {
                            if (this.dropzone.files.length > 1) {
                                if (this.dropzone.files[0] !== file) {
                                    this.dropzone.removeFile(this.dropzone.files[0]);
                                }
                            }

                            const reader = new FileReader();
                            reader.onload = (e) => {
                                this.uploadedFile = {
                                    name: file.name,
                                    size: file.size,
                                    dataUrl: e.target.result
                                };
                            };
                            reader.readAsDataURL(file);
                        });

                        this.dropzone.on('removedfile', (file) => {
                            this.uploadedFile = null;
                            const imageInput = document.querySelector('input[name="Images"]');
                            if (imageInput) {
                                imageInput.remove();
                            }
                        });

                        this.dropzone.on('success', (file, response) => {
                            if (response.success) {
                                const imagePathInput = document.createElement('input');
                                imagePathInput.type = 'hidden';
                                imagePathInput.name = 'Images';
                                imagePathInput.value = response.fileName;
                                document.getElementById('receptionForm').appendChild(imagePathInput);
                            } else {
                                this.dropzone.removeFile(file);
                                this.uploadedFile = null;
                                notyf.error(response.message || 'Có lỗi xảy ra khi upload ảnh');
                            }
                        });

                        this.dropzone.on('error', (file, errorMessage) => {
                            this.dropzone.removeFile(file);
                            this.uploadedFile = null;
                            notyf.error(errorMessage);
                        });
                    }
                });
            } catch (error) {
                console.error('Error initializing Dropzone:', error);
            }
        },

        setupChoices() {
            const selectElements = document.querySelectorAll('select.form-input');
            selectElements.forEach(select => {
                new Choices(select, {
                    searchEnabled: true,
                    searchPlaceholderValue: 'Tìm kiếm...',
                    removeItemButton: true,
                    noResultsText: 'Không tìm thấy kết quả',
                    noChoicesText: 'Không có lựa chọn nào',
                    itemSelectText: ''
                });
            });
        },

        setupDropzone() {
            setTimeout(() => {
                this.reinitializeDropzone();
            }, 100);
        },

        setupDateTimePicker() {
            flatpickr('.flatpickr', {
                dateFormat: "d/m/Y",
                allowInput: true
            });
        },

        setupValidation() {
            $("#receptionForm").validate({
                ignore: [],
                rules: {
                    "Name": { required: true, minlength: 2, maxlength: 50 },
                    "DateOfBirth": { required: true, dateFormat: true },
                    "Gender": { required: true },
                    "IdentityNumber": { required: true, minlength: 9, maxlength: 12 },
                    "PhoneNumber": { required: true, phone: true },
                    "Address": { required: true, minlength: 5, maxlength: 500 },
                    "Email": { email: true },
                    "HealthInsuranceNumber": { required: function() { return $('#HasHealthInsurance').is(':checked'); } },
                    "HealthInsuranceExpiryDate": { required: function() { return $('#HasHealthInsurance').is(':checked'); }, dateFormat: true },
                    "HealthInsurancePlaceOfRegistration": { required: function() { return $('#HasHealthInsurance').is(':checked'); } },
                    "Diagnosis": { required: true },
                    "StartDate": { required: true, dateFormat: true },
                    "EndDate": { required: true, dateFormat: true },
                    "TreatmentMethodId": { required: true },
                    "RoomId": { required: true },
                    "StartDate": { required: true, dateFormat: true },
                    "EndDate": { required: true, dateFormat: true }
                },
                messages: {
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
                    "TreatmentMethodId": { required: "Phương pháp điều trị không được bỏ trống." },
                    "RoomId": { required: "Phòng điều trị không được bỏ trống." },
                    "StartDate": {
                        required: "Ngày bắt đầu không được bỏ trống.",
                        dateFormat: "Ngày bắt đầu không hợp lệ."
                    },
                    "EndDate": {
                        required: "Ngày kết thúc không được bỏ trống.",
                        dateFormat: "Ngày kết thúc không hợp lệ."
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
                onfocusout: function(element) {
                    if ($(element).val() === '' || $(element).val().length > 0) {
                        this.element(element);
                    }
                },
                onkeyup: false
            });
            $('select.form-input').on('change', function() {
                $(this).valid();
            });
        },

        filterRooms(treatmentMethodId) {
            if (!treatmentMethodId) {
                this.filteredRooms = [];
                return;
            }
            this.filteredRooms = this.allRooms.filter(room => room.TreatmentMethodId == treatmentMethodId);
        },

        handleResponse(response) {
            const overlay = document.getElementById('loadingOverlay');
            if (response.success) {
                overlay.style.display = 'flex';
                notyf.success(response.message);
                setTimeout(() => {
                    window.location.href = '/Staff/Receptions/';
                }, 2000);
            } else {
                overlay.style.display = 'none';
                notyf.error(response.message);
                if (response.errors) {
                    response.errors.forEach(error => notyf.error(error));
                }
            }
        },

        submitForm() {
            if ($("#receptionForm").valid()) {
                const overlay = document.getElementById('loadingOverlay');
                overlay.style.display = 'flex';

                if (this.dropzone && this.dropzone.files.length > 0) {
                    this.dropzone.processQueue();
                }

                const formData = new FormData(document.getElementById('receptionForm'));

                fetch('/Staff/Receptions/Create', {
                    method: 'POST',
                    body: formData
                })
                .then(response => response.json())
                .then(this.handleResponse)
                .catch(error => {
                    overlay.style.display = 'none';
                    notyf.error("Có lỗi xảy ra khi gửi yêu cầu: " + error);
                });
            } else {
                notyf.error("Vui lòng kiểm tra lại thông tin đã nhập.");
            }
        }
    }));
});

$.validator.addMethod("dateFormat", function(value, element) {
    return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
}, "Vui lòng nhập ngày hợp lệ (dd/mm/yyyy).");

$.validator.addMethod("phone", function(value, element) {
    return this.optional(element) || /^\+?\d{10,12}$/.test(value);
}, "Số điện thoại không hợp lệ.");