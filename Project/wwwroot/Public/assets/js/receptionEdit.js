// Disable auto-discovery for Dropzone
Dropzone.autoDiscover = false;

/**
 * Modal Management Module
 */
const ModalManager = {
    /**
     * Show note modal with content
     * @param {string} note - The note content to display
     * @param {string} type - The type of note ('treatment' or 'assignment')
     * @param {string} identifier - The identifier for the note
     */
    showNoteModal: function(note, type, identifier) {
        const modal = document.getElementById('noteModal');
        const noteContent = document.getElementById('noteContent');
        const modalTitle = document.getElementById('noteModalTitle');

        // Set modal title based on type
        const titles = {
            'treatment': `Ghi chú của chi tiết điều trị (${identifier})`,
            'assignment': `Ghi chú của bác sĩ (${identifier})`,
            'default': 'Ghi chú'
        };
        modalTitle.textContent = titles[type] || titles.default;

        // Set note content
        if (!note?.trim()) {
            noteContent.textContent = 'Chưa thêm ghi chú';
            noteContent.classList.add('text-gray-500', 'italic');
        } else {
            noteContent.textContent = note;
            noteContent.classList.remove('text-gray-500', 'italic');
        }

        // Show modal with animation
        modal.classList.remove('hidden');
        const modalContent = modal.querySelector('.modal-content');
        modalContent.style.animation = 'none';
        modalContent.offsetHeight; // Trigger reflow
        modalContent.style.animation = null;
    },

    /**
     * Close note modal with animation
     */
    closeNoteModal: function() {
        const modal = document.getElementById('noteModal');
        const modalContent = modal.querySelector('.modal-content');
        modalContent.classList.add('modal-closing');

        setTimeout(() => {
            modal.classList.add('hidden');
            modalContent.classList.remove('modal-closing');
        }, 200);
    },

    /**
     * Initialize modal event listeners
     */
    init: function() {
        // Close modal when clicking outside
        window.onclick = (event) => {
            const modal = document.getElementById('noteModal');
            if (event.target === modal) {
                this.closeNoteModal();
            }
        };
    }
};

// Make modal functions globally available
window.showNoteModal = ModalManager.showNoteModal;
window.closeNoteModal = ModalManager.closeNoteModal;

/**
 * Form Management Module
 */
const FormManager = {
    /**
     * Append form data for submission
     * @param {FormData} formData - The FormData object to append to
     */
    appendFormData: function(formData) {
        // Patient data
        const patientFields = [
            'Code', 'Name', 'Gender', 'DateOfBirth', 'IdentityNumber',
            'Address', 'PhoneNumber', 'Email'
        ];
        patientFields.forEach(field => {
            const value = document.getElementById(field)?.value;
            if (value) formData.append(`Patient.${field}`, value);
        });

        const hasInsurance = document.getElementById('HasHealthInsurance').checked;
        formData.append('Patient.HasHealthInsurance', hasInsurance);

        // Health insurance data
        if (hasInsurance) {
            const insuranceFields = [
                'HealthInsuranceCode', 'HealthInsuranceNumber',
                'HealthInsuranceExpiryDate', 'HealthInsurancePlaceOfRegistration'
            ];
            insuranceFields.forEach(field => {
                const value = document.getElementById(field)?.value;
                if (value) formData.append(`Patient.${field}`, value);
            });
        }

        // Treatment record detail data
        const treatmentDetailFields = {
            'treatmentRecordDetailCode': 'Code',
            'treatmentRecordDetailTreatmentMethod': 'TreatmentMethodId',
            'treatmentRecordDetailRoom': 'RoomId',
            'treatmentRecordDetailNote': 'Note'
        };
        Object.entries(treatmentDetailFields).forEach(([elementId, fieldName]) => {
            const value = document.getElementById(elementId)?.value;
            if (value) formData.append(`NewTreatmentRecordDetail.${fieldName}`, value);
        });

        // Assignment data
        const assignmentFields = {
            'assignmentCode': 'Code',
            'assignmentStartDate': 'StartDate',
            'assignmentEndDate': 'EndDate',
            'assignmentNote': 'Note'
        };
        Object.entries(assignmentFields).forEach(([elementId, fieldName]) => {
            const value = document.getElementById(elementId)?.value;
            if (value) formData.append(`NewAssignment.${fieldName}`, value);
        });

        // Regulations data
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

        // Add antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        formData.append('__RequestVerificationToken', token);
    }
};

/**
 * Reception Edit Module
 */
document.addEventListener('alpine:init', () => {
    console.log('Alpine.js initialized');
    Alpine.data('receptionEdit', () => ({
        // State
        dropzone: null,
        filteredRooms: [],
        allRooms: [],
        roomChoices: null,
        regulations: [],
        availableRegulations: [],
        hasHealthInsurance: window.hasHealthInsurance || false,
        images: [],
        errors: {},
        choicesInstances: {},
        assignmentStartPicker: null,
        assignmentEndPicker: null,
        showAddTreatmentForm: false,
        showAddAssignmentForm: false,
        treatmentRecord: window.treatmentRecord || null,
        assignments: window.assignments || [],
        tab: 'treatment-record',

        /**
         * Initialize the component
         */
        init() {
            console.log('Component initialized');
            try {
                this.filteredRooms = [];
                this.allRooms = window.roomsData || [];
                this.availableRegulations = window.regulationsData || [];
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
            console.log('Initializing components...');
            this.setupDropzone();
            this.setupValidation();
            this.setupDateTimePicker();
            this.setupChoices();
            this.setupAssignmentDatePickers();
        },

        /**
         * Set up Dropzone for image uploads
         */
        setupDropzone() {
            const dropzone = new Dropzone("#imageDropzone", {
                url: "/Staff/Receptions/Edit",
                maxFiles: 1,
                acceptedFiles: "image/*",
                maxFilesize: 5,
                addRemoveLinks: false,
                createImageThumbnails: true,
                previewTemplate: document.querySelector('.dz-preview-container').innerHTML,
                dictDefaultMessage: "",
                autoProcessQueue: false,
                init: function() {
                    const dz = this;
                    const previewContainer = document.querySelector('.dz-preview-container');
                    const messageContainer = document.querySelector('.dz-message');

                    if (existingImage) {
                        const mockFile = { name: "Existing Image", size: 0 };
                        dz.displayExistingFile(mockFile, existingImage);
                        previewContainer.querySelector('img').src = existingImage;
                        previewContainer.classList.remove('hidden');
                        messageContainer.classList.add('hidden');
                    }

                    dz.on("addedfile", function(file) {
                        if (dz.files.length > 1) {
                            dz.removeFile(dz.files[0]);
                        }
                        
                        const reader = new FileReader();
                        reader.onload = function(e) {
                            previewContainer.querySelector('img').src = e.target.result;
                            previewContainer.querySelector('.preview-filename').textContent = file.name;
                            previewContainer.querySelector('.preview-size').textContent = 
                                (file.size / (1024 * 1024)).toFixed(2) + ' MB';
                            previewContainer.classList.remove('hidden');
                            messageContainer.classList.add('hidden');
                        };
                        reader.readAsDataURL(file);
                    });

                    dz.on("error", function(file, errorMessage) {
                        const errorElement = document.createElement('div');
                        errorElement.className = 'text-red-500 text-sm mt-2';
                        errorElement.textContent = errorMessage;
                        file.previewElement.appendChild(errorElement);
                        setTimeout(() => dz.removeFile(file), 3000);
                    });

                    previewContainer.querySelector('.remove-file').addEventListener('click', function() {
                        dz.removeAllFiles();
                        previewContainer.classList.add('hidden');
                        messageContainer.classList.remove('hidden');
                    });

                    document.querySelector('#fileInput').addEventListener('change', function(e) {
                        if (e.target.files && e.target.files[0]) {
                            dz.addFile(e.target.files[0]);
                        }
                    });
                }
            });

            return dropzone;
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

        // ... rest of your existing methods ...

    }));
});

// Initialize modal manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    ModalManager.init();
}); 