Dropzone.autoDiscover = false;

document.addEventListener('alpine:init', () => {
    Alpine.data('editData', () => ({
        dropzone: null,
        tab: 'treatment-record',
        hasHealthInsurance: false,
        showAddTreatmentForm: false,
        showAddAssignmentForm: false,
        regulations: [],
        allRegulations: [],

        displayNote(note, type, identifier) {
            let title = '';
            if (type === 'treatment') {
                title = `Ghi chú điều trị - ${identifier}`;
            } else if (type === 'assignment') {
                title = `Ghi chú phân công - ${identifier}`;
            } else if (type === 'regulation') {
                title = `Ghi chú quy định - ${identifier}`;
            } else {
                title = 'Ghi chú';
            }

            $.confirm({
                title: title,
                content: note || 'Không có ghi chú',
                type: 'blue',
                typeAnimated: true,
                theme: 'modern',
                boxWidth: '800px',
                useBootstrap: false,
                draggable: false,
                buttons: {
                    close: {
                        text: 'Đóng',
                        btnClass: 'btn-blue',
                        action: function() {}
                    }
                }
            });
        },

        init() {
            this.setupDropzone();
            this.setupFlatpickr();
            this.setupChoices();
            this.setupValidation();
            
            // Initialize hasHealthInsurance from template data
            const healthInsuranceNumber = document.querySelector('[name="Patient.HealthInsuranceNumber"]')?.value;
            const healthInsuranceCode = document.querySelector('[name="Patient.HealthInsuranceCode"]')?.value;
            this.hasHealthInsurance = !!(healthInsuranceNumber || healthInsuranceCode);
            $('select').on('focus', function () {
                $(this).trigger('click');
            });
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
                removeItemButton: true,
                noResultsText: 'Không tìm thấy kết quả',
                noChoicesText: 'Không có phương pháp điều trị nào',
                itemSelectText: ''
            });

            this.loadAvailableTreatmentMethods(treatmentMethodChoices);
        },

        loadAvailableTreatmentMethods(treatmentMethodChoices) {
            const treatmentRecordId = document.getElementById('treatmentRecordId').value;
            const treatmentMethodWarning = document.getElementById('treatmentMethodWarning');
            const treatmentMethodSelect = document.getElementById('treatmentRecordDetailTreatmentMethod');
            
            fetch(`/api/Utils/GetAvailableTreatmentMethods?treatmentRecordId=${treatmentRecordId}`)
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
            const treatmentStartDateInput = document.getElementById('StartDate');
            const treatmentEndDateInput = document.getElementById('EndDate');
            const assignmentStartDateInput = document.getElementById('assignmentStartDate');
            const assignmentEndDateInput = document.getElementById('assignmentEndDate');
            const assignmentDateWarning = document.getElementById('assignmentDateWarning');

            // Function to check and update assignment date inputs state
            const updateAssignmentDateState = () => {
                const treatmentStartDate = treatmentStartDateInput.value;
                const treatmentEndDate = treatmentEndDateInput.value;
                const hasValidTreatmentDates = treatmentStartDate && treatmentEndDate;

                // Enable/disable assignment date inputs
                assignmentStartDateInput.disabled = !hasValidTreatmentDates;
                assignmentEndDateInput.disabled = !hasValidTreatmentDates;

                // Remove readonly attribute - it should not be there
                assignmentStartDateInput.removeAttribute('readonly');
                assignmentEndDateInput.removeAttribute('readonly');

                // Show/hide warning message
                assignmentDateWarning.style.display = hasValidTreatmentDates ? 'none' : 'block';

                // Update visual state of inputs
                [assignmentStartDateInput, assignmentEndDateInput].forEach(input => {
                    if (hasValidTreatmentDates) {
                        input.classList.remove('opacity-50', 'cursor-not-allowed');
                    } else {
                        input.classList.add('opacity-50', 'cursor-not-allowed');
                        input.value = '';
                    }
                });
            };

            // Initial state check
            updateAssignmentDateState();

            // Setup flatpickr for treatment dates
            flatpickr(treatmentStartDateInput, {
                dateFormat: "d/m/Y",
                onChange: function(selectedDates, dateStr) {
                    updateAssignmentDateState();
                }
            });

            flatpickr(treatmentEndDateInput, {
                dateFormat: "d/m/Y",
                onChange: function(selectedDates, dateStr) {
                    updateAssignmentDateState();
                }
            });

            // Setup flatpickr for assignment dates - without readonly
            flatpickr(assignmentStartDateInput, {
                dateFormat: "d/m/Y",
                allowInput: true
            });

            flatpickr(assignmentEndDateInput, {
                dateFormat: "d/m/Y",
                allowInput: true
            });
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
                init: function() {
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
            fetch(`/api/Utils/GetRoomsByTreatmentMethod?id=${treatmentMethodId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(rooms => {
                    console.log('Filtered rooms:', rooms);
                    
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
            // Get form data
            const form = document.getElementById('receptionForm');
            const formDataObj = new FormData(form);
            
            // Append all form fields to the request
            for (let [key, value] of formDataObj.entries()) {
                formData.append(key, value);
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

            $.validator.addMethod("notPastDate", function(value, element) {
                if (!value) return true;
                
                var dateParts = value.split("/");
                var inputDate = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
                var today = new Date();
                today.setHours(0, 0, 0, 0);
                
                return inputDate >= today;
            }, "Ngày bắt đầu không được là ngày trong quá khứ");

            $.validator.addMethod("endDateAfterStartDate", function(value, element) {
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

            $.validator.addMethod("over14", function(value, element) {
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
            $.validator.addMethod("assignmentStartDateValid", function(value, element) {
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

            $.validator.addMethod("assignmentEndDateValid", function(value, element) {
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
                        required: function() {
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
                                return JSON.parse(data) === true;
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
                    "Patient.HealthInsuranceExpiryDate": {
                        required: () => $('#HasHealthInsurance').is(':checked'),
                        dateFormat: true,
                        notExpired: true
                    },
                    "Patient.HealthInsurancePlaceOfRegistration": {
                        required: () => $('#HasHealthInsurance').is(':checked')
                    },
                    "NewTreatmentRecordDetail.TreatmentMethodId": {
                        required: function() {
                            return $("#showAddTreatmentForm").is(":visible");
                        }
                    },
                    "NewTreatmentRecordDetail.RoomId": {
                        required: function() {
                            return $("#showAddTreatmentForm").is(":visible");
                        }
                    },
                    "NewAssignment.StartDate": {
                        required: function() {
                            return $("[x-show='showAddAssignmentForm']").is(":visible");
                        },
                        dateFormat: true,
                        assignmentStartDateValid: true
                    },
                    "NewAssignment.EndDate": {
                        required: function() {
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
            $("#receptionForm").on('submit', function(e) {
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
            const existingRegulationIds = this.regulations.map(r => r.RegulationId);
            return this.allRegulations.filter(r => !existingRegulationIds.includes(r.id));
        },

        /**
         * Get available regulations for a specific index
         */
        getAvailableRegulations(currentIndex) {
            const existingRegulationIds = this.regulations
                .map((r, index) => index !== currentIndex ? r.RegulationId : null)
                .filter(id => id !== null);
            return this.allRegulations.filter(r => !existingRegulationIds.includes(r.id));
        },

        /**
         * Add a new regulation
         */
        addRegulation() {
            if (this.regulations.length >= 5) {
                notyf.error('Không thể thêm quá 5 quy định cho một phiếu điều trị');
                return;
            }

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

            this.$nextTick(() => {
                const index = this.regulations.length - 1;
                const selectElement = document.getElementById(`regulationId-${index}`);
                if (selectElement) {
                    const choices = new Choices(selectElement, {
                        removeItemButton: true,
                        searchEnabled: true,
                        placeholder: true,
                        placeholderValue: 'Chọn quy định',
                        noResultsText: 'Không tìm thấy kết quả',
                        itemSelectText: '',
                        choices: availableRegs.map(reg => ({
                            value: reg.id.toString(),
                            label: reg.name
                        }))
                    });

                    selectElement.addEventListener('change', (e) => {
                        this.onRegulationChange(index, e.target.value);
                    });
                }

                const dateElement = document.getElementById(`executionDate-${index}`);
                if (dateElement) {
                    this.initRegulationDatePicker(dateElement);
                }
            });
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