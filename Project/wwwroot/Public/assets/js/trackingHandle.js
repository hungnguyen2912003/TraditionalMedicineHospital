document.addEventListener('alpine:init', () => {
    Alpine.data('list', () => ({
        entityList: window.trackingModel || [],
        filteredList: [],
        selectedRows: [],
        datatable: null,

        init() {
            if (!Array.isArray(this.entityList)) {
                this.entityList = [];
            }
            this.filteredList = [...this.entityList];
            window.entityList = this.entityList;
            window.list = this;
            this.initializeTable();
        },

        initializeTable() {
            try {
                const tableData = this.filteredList.map(entity => [
                    `<input type="checkbox" class="form-checkbox" value="${entity.id}" ${this.selectedRows.includes(entity.id.toString()) ? 'checked' : ''} />`,
                    entity.code,
                    entity.patientName,
                    new Date(entity.trackingDate).toLocaleDateString('vi-VN'),
                    this.getTrackingStatusBadge(entity.status),
                    `<div class="flex gap-4">
                        <a href="javascript:;"
                        class="edit-btn"
                        data-id="${entity.id}"
                        data-status="${entity.status}"
                        data-note="${entity.note || ''}"
                        data-doctor-code="${entity.employeeCode}"
                        data-treatmentrecord-status="${entity.treatmentRecordStatus}">
                            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-4.5 h-4.5">
                                <path opacity="0.5" d="M22 10.5V12C22 16.714 22 19.0711 20.5355 20.5355C19.0711 22 16.714 22 12 22C7.28595 22 4.92893 22 3.46447 20.5355C2 19.0711 2 16.714 2 12C2 7.28595 2 4.92893 3.46447 3.46447C4.92893 2 7.28595 2 12 2H13.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"></path>
                                <path d="M17.3009 2.80624L16.652 3.45506L10.6872 9.41993C10.2832 9.82394 10.0812 10.0259 9.90743 10.2487C9.70249 10.5114 9.52679 10.7957 9.38344 11.0965C9.26191 11.3515 9.17157 11.6225 8.99089 12.1646L8.41242 13.9L8.03811 15.0229C7.9492 15.2897 8.01862 15.5837 8.21744 15.7826C8.41626 15.9814 8.71035 16.0508 8.97709 15.9619L10.1 15.5876L11.8354 15.0091C12.3775 14.8284 12.6485 14.7381 12.9035 14.6166C13.2043 14.4732 13.4886 14.2975 13.7513 14.0926C13.9741 13.9188 14.1761 13.7168 14.5801 13.3128L20.5449 7.34795L21.1938 6.69914C22.2687 5.62415 22.2687 3.88124 21.1938 2.80624C20.1188 1.73125 18.3759 1.73125 17.3009 2.80624Z" stroke="currentColor" stroke-width="1.5"></path>
                                <path opacity="0.5" d="M16.6522 3.45508C16.6522 3.45508 16.7333 4.83381 17.9499 6.05034C19.1664 7.26687 20.5451 7.34797 20.5451 7.34797M10.1002 15.5876L8.4126 13.9" stroke="currentColor" stroke-width="1.5"></path>
                            </svg>
                        </a>
                        <button type="button" class="hover:text-danger delete-btn" data-id="${entity.id}" data-treatmentrecord-status="${entity.treatmentRecordStatus}" x-tooltip="Đưa vào thùng rác">
                            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-5 h-5">
                                <path d="M20.5001 6H3.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"></path>
                                <path d="M18.8334 8.5L18.3735 15.3991C18.1965 18.054 18.108 19.3815 17.243 20.1907C16.378 21 15.0476 21 12.3868 21H11.6134C8.9526 21 7.6222 21 6.75719 20.1907C5.89218 19.3815 5.80368 18.054 5.62669 15.3991L5.16675 8.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"></path>
                                <path opacity="0.5" d="M9.5 11L10 16" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"></path>
                                <path opacity="0.5" d="M14.5 11L14 16" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"></path>
                                <path opacity="0.5" d="M6.5 6C6.55588 6 6.58382 6 6.60915 5.99936C7.43259 5.97849 8.15902 5.45491 8.43922 4.68032C8.44784 4.65649 8.45667 4.62999 8.47434 4.57697L8.57143 4.28571C8.65431 4.03708 8.69575 3.91276 8.75071 3.8072C8.97001 3.38607 9.37574 3.09364 9.84461 3.01877C9.96213 3 10.0932 3 10.3553 3H13.6447C13.9068 3 14.0379 3 14.1554 3.01877C14.6243 3.09364 15.03 3.38607 15.2493 3.8072C15.3043 3.91276 15.3457 4.03708 15.4286 4.28571L15.5257 4.57697C15.5433 4.62992 15.5522 4.65651 15.5608 4.68032C15.841 5.45491 16.5674 5.97849 17.3909 5.99936C17.4162 6 17.4441 6 17.5 6" stroke="currentColor" stroke-width="1.5"></path>
                            </svg>
                        </button>
                    </div>`
                ]);

                this.datatable = new simpleDatatables.DataTable('#myTable', {
                    data: {
                        headings: [
                            '<input type="checkbox" class="form-checkbox" />',
                            'Mã theo dõi điều trị',
                            'Tên bệnh nhân',
                            'Ngày theo dõi',
                            'Trạng thái',
                            'Thao tác'
                        ],
                        data: tableData
                    },
                    perPage: 10,
                    perPageSelect: [10, 20, 30, 50, 100],
                    firstLast: true,
                    searchable: false,
                    firstText: '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-4.5 h-4.5 rtl:rotate-180"><path d="M13 19L7 12L13 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/><path opacity="0.5" d="M16.9998 19L10.9998 12L16.9998 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    lastText: '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-4.5 h-4.5 rtl:rotate-180"><path d="M11 19L17 12L11 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/><path opacity="0.5" d="M6.99976 19L12.9998 12L6.99976 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    prevText: '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-4.5 h-4.5 rtl:rotate-180"><path d="M15 5L9 12L15 19" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    nextText: '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="w-4.5 h-4.5 rtl:rotate-180"><path d="M9 5L15 12L9 19" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    labels: {
                        perPage: "<span class='ml-2'>{select}</span>",
                        noRows: 'Không có dữ liệu'
                    },
                    layout: {
                        top: '{search}',
                        bottom: '{info}{select}{pager}'
                    },
                    columns: [
                        {
                            select: 0,
                            sortable: false,
                            render: (data, cell, row) => {
                                return data;
                            }
                        },
                        {
                            select: 3,
                            sortable: true,
                            render: (data, cell, row) => {
                                return data;
                            }
                        },
                        {
                            select: 4,
                            sortable: false,
                            render: (data, cell, row) => {
                                return data;
                            }
                        },
                        {
                            select: 5,
                            sortable: false,
                            render: (data, cell, row) => {
                                return data;
                            }
                        }
                    ],
                });

                // Gán lại sự kiện cho checkbox và delete-btn
                this.rebindCheckboxEvents();
                // Gán lại sự kiện khi chuyển trang
                this.datatable.on('datatable.page', () => {
                    this.rebindCheckboxEvents();
                });
            } catch (error) {
                console.error('Error initializing table:', error);
            }
        },

        rebindCheckboxEvents() {
            // Gán lại sự kiện cho nút xóa
            document.querySelectorAll('.delete-btn').forEach(btn => {
                btn.onclick = function () {
                    const id = this.getAttribute('data-id');
                    const treatmentRecordStatus = $(this).data('treatmentrecord-status');
                    if (treatmentRecordStatus == 2 || treatmentRecordStatus == 3) {
                        notyf.error('Phiếu điều trị này đã hoàn thành hoặc đã hủy, không thể thao tác!');
                        return;
                    }
                    if (window.list && typeof window.list.delete === 'function') {
                        window.list.delete(id);
                    }
                };
            });

            // Gán lại sự kiện cho checkbox "chọn tất cả"
            const checkAll = document.querySelector('#myTable thead input[type="checkbox"]');
            const rowCheckboxes = document.querySelectorAll('#myTable tbody input[type="checkbox"]');
            const treatmentRecordStatus = $(this).data('treatmentrecord-status');
            if (checkAll) {
                checkAll.onclick = function () {
                    if (checkAll.checked) {
                        // Chỉ chọn các bản ghi đang hiển thị (filteredList)
                        window.list.selectedRows = window.list.filteredList.map(e => e.id.toString());
                    } else {
                        // Bỏ chọn tất cả các bản ghi đang hiển thị
                        window.list.selectedRows = window.list.selectedRows.filter(id =>
                            !window.list.filteredList.some(e => e.id.toString() === id)
                        );
                    }
                    // Cập nhật lại trạng thái checkbox trên trang hiện tại
                    rowCheckboxes.forEach(cb => {
                        cb.checked = checkAll.checked;
                    });
                };
            }
            // Gán lại sự kiện cho từng checkbox dòng
            rowCheckboxes.forEach(cb => {
                cb.checked = window.list.selectedRows.includes(cb.value);
                cb.onclick = function () {
                    const id = cb.value;
                    if (cb.checked) {
                        if (!window.list.selectedRows.includes(id)) window.list.selectedRows.push(id);
                    } else {
                        window.list.selectedRows = window.list.selectedRows.filter(x => x !== id);
                        if (checkAll) checkAll.checked = false;
                    }
                };
            });
        },

        deleteSelected() {
            if (this.selectedRows.length === 0) {
                notyf.error('Vui lòng chọn ít nhất một bản ghi để xóa.');
                return;
            }

            const treatmentRecordStatus = $(this).data('treatmentrecord-status');
            if (treatmentRecordStatus == 2 || treatmentRecordStatus == 3) {
                notyf.error('Phiếu điều trị này đã hoàn thành hoặc đã hủy, không thể cập nhật!');
                return;
            }

            $.confirm({
                title: 'Bạn có chắc chắn?',
                content: 'Bạn có muốn xóa các bản ghi theo dõi điều trị đã chọn không?',
                icon: 'fa fa-question-circle text-blue-500',
                theme: 'modern',
                type: 'blue',
                boxWidth: '400px',
                useBootstrap: false,
                buttons: {
                    confirm: {
                        text: 'Có, hãy thực hiện!',
                        btnClass: 'btn-success',
                        action: () => {
                            document.getElementById('selectedIds').value = this.selectedRows.join(',');
                            document.getElementById('myForm').action = '/theo-doi-dieu-tri/xoa';
                            document.getElementById('myForm').submit();
                        }
                    },
                    cancel: {
                        text: 'Hủy',
                        btnClass: 'btn-secondary'
                    }
                }
            });
        },

        delete(id) {
            $.confirm({
                title: 'Bạn có chắc chắn?',
                content: 'Bạn có muốn xóa bản ghi theo dõi điều trị này không?',
                icon: 'fa fa-question-circle text-blue-500',
                theme: 'modern',
                type: 'blue',
                boxWidth: '400px',
                useBootstrap: false,
                buttons: {
                    confirm: {
                        text: 'Có, hãy thực hiện!',
                        btnClass: 'btn-success',
                        action: () => {
                            document.getElementById('selectedIds').value = id;
                            document.getElementById('myForm').action = '/theo-doi-dieu-tri/xoa';
                            document.getElementById('myForm').submit();
                        }
                    },
                    cancel: {
                        text: 'Hủy',
                        btnClass: 'btn-secondary'
                    }
                }
            });
        },

        updateTable() {
            if (this.datatable) {
                this.datatable.destroy();
                this.datatable = null;
            }
            this.initializeTable();
        },

        openTrackingModal() {
            const today = new Date();
            const day = String(today.getDate()).padStart(2, '0');
            const month = String(today.getMonth() + 1).padStart(2, '0');
            const year = today.getFullYear();
            const dateStr = `${year}-${month}-${day}`;

            $('#trackingModal').removeClass('hidden closing');
            $('.modal-content-animate', '#trackingModal').removeClass('animate__fadeOut');
            $('#modalRoomTitle').html(`Ngày ${day}/${month}/${year} – Phòng ...`);

            $.get('/api/trackinghandles/room-name', function (data) {
                $('#modalRoomTitle').html(`Ngày ${day}/${month}/${year} – Phòng ${data.roomName}`);
                $('#trackingModal').data('room-id', data.roomId);
            });

            $.get(`/api/trackinghandles/patients-in-room?date=${dateStr}`, function (patients) {
                // Lọc lại: chỉ lấy bệnh nhân không có 3 bản ghi liên tiếp 'Không điều trị'
                const entityList = window.entityList || [];
                const filteredPatients = patients.filter(patient => {
                    // Lấy tất cả bản ghi của bệnh nhân này, sắp xếp theo ngày tăng dần
                    const trackings = entityList
                        .filter(e => e.patientName === patient.patientName)
                        .sort((a, b) => new Date(a.trackingDate) - new Date(b.trackingDate));
                    // Kiểm tra nếu có chuỗi 3 bản ghi liên tiếp 'Không điều trị' thì loại bỏ
                    for (let i = 0; i < trackings.length - 2; i++) {
                        if (
                            trackings[i].status === 3 &&
                            trackings[i + 1].status === 3 &&
                            trackings[i + 2].status === 3
                        ) {
                            return false;
                        }
                    }
                    return true;
                });
                let rows = '';
                if (filteredPatients.length === 0) {
                    rows = `<tr><td colspan="5" style="text-align:center;color:#888;font-style:italic;">Không còn bệnh nhân nào cần chấm theo dõi trong ngày hôm nay</td></tr>`;
                } else {
                    filteredPatients.forEach((patient, idx) => {
                        rows += `
                            <tr data-patient-name="${patient.patientName}" data-patient-id="${patient.patientId}">
                                <td>${idx + 1}</td>
                                <td>${patient.patientName}</td>
                                <td>
                                    <div class="btn-group d-flex justify-content-center" style="display:flex;gap:8px;">
                                        <button class="btn btn-success btn-status" data-status="1">Có điều trị</button>
                                        <button class="btn btn-warning btn-status" data-status="2">Xin phép</button>
                                        <button class="btn btn-danger btn-status" data-status="3">Không điều trị</button>
                                    </div>
                                </td>
                                <td>
                                    <input type="text" class="form-control note-input" placeholder="Nhập ghi chú..." />
                                </td>
                                <td>
                                    <button class="btn btn-primary btn-save">Lưu</button>
                                </td>
                            </tr>
                        `;
                    });
                }
                $('#modalPatientTableBody').html(rows);
            });
        },

        getTrackingStatusBadge(status) {
            let text = getEnumDisplayName('TrackingStatus', status);
            let style = '';
            switch (status) {
                case 1: // Có điều trị
                    style = 'background:#22c55e;color:white;'; // xanh lá cây
                    break;
                case 2: // Xin phép
                    style = 'background:#f59e42;color:white;'; // vàng cam
                    break;
                case 3: // Không điều trị
                    style = 'background:#ef4444;color:white;'; // đỏ
                    break;
                default:
                    style = 'background:#a3a3a3;color:white;'; // xám
            }
            return `<span style="display:inline-block;min-width:80px;text-align:center;${style}border-radius:6px;padding:4px 12px;font-size:13px;font-weight:bold;">${text}</span>`;
        },

        filterList(patient, date, status, dateFrom, dateTo, showNotyf = true) {
            let filtered = this.entityList;
            if (patient) {
                filtered = filtered.filter(e => e.patientName === patient);
            }
            if (date) {
                const [d, m, y] = date.split('/');
                const filterDate = new Date(`${y}-${m}-${d}`).toISOString().slice(0, 10);
                filtered = filtered.filter(e => {
                    const entityDate = new Date(e.trackingDate).toISOString().slice(0, 10);
                    return entityDate === filterDate;
                });
            }
            if (status) {
                filtered = filtered.filter(e => String(e.status) === String(status));
            }
            if (dateFrom || dateTo) {
                filtered = filtered.filter(e => {
                    const entityDate = new Date(e.trackingDate);
                    const entityDateStr = entityDate.toISOString().slice(0, 10);
                    let fromStr = dateFrom ? dateFrom.split('/').reverse().join('-') : null;
                    let toStr = dateTo ? dateTo.split('/').reverse().join('-') : null;
                    if (fromStr && entityDateStr < fromStr) return false;
                    if (toStr && entityDateStr > toStr) return false;
                    return true;
                });
            }
            this.filteredList = filtered;
            if (this.datatable) {
                this.datatable.destroy();
            }
            this.initializeTable();
            if (showNotyf && filtered.length > 0) {
                notyf.success('Lọc thành công!');
            }
        }
    }));
});

$(document).on('click', '.btn-status', function () {
    var $row = $(this).closest('tr');
    if ($(this).hasClass('active-status')) {
        // Nếu đã active thì bỏ chọn
        $(this).removeClass('active-status');
    } else {
        // Nếu chưa active thì chuyển active
        $row.find('.btn-status').removeClass('active-status');
        $(this).addClass('active-status');
    }
});

$(document).on('click', '.btn-save', function (e) {
    e.preventDefault();
    var $row = $(this).closest('tr');
    var patientId = $row.data('patient-id');
    var roomId = $('#trackingModal').data('room-id');
    var statusBtn = $row.find('.btn-status.active-status');
    if (statusBtn.length === 0) {
        notyf.error('Vui lòng chọn trạng thái!');
        return;
    }
    var status = statusBtn.data('status');
    var note = $row.find('.note-input').val();
    var trackingDate = new Date().toISOString();
    const overlay = document.getElementById('loadingOverlay');

    overlay.style.display = 'flex';

    fetch('/api/trackinghandles', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            patientId: patientId,
            roomId: roomId,
            status: status,
            note: note,
            trackingDate: trackingDate
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success !== false) {
                overlay.style.display = 'none';
                notyf.success('Lưu thành công!');
                // Xóa dòng bệnh nhân vừa lưu khỏi bảng
                $row.remove();
                // Cập nhật lại số thứ tự (STT)
                $('#modalPatientTableBody tr').each(function (index) {
                    $(this).find('td').eq(0).text(index + 1);
                });
                // Nếu không còn bệnh nhân nào, hiển thị dòng thông báo
                if ($('#modalPatientTableBody tr').length === 0) {
                    $('#modalPatientTableBody').html(
                        `<tr><td colspan="5" style="text-align:center;color:#888;font-style:italic;">
                            Không còn bệnh nhân nào cần chấm theo dõi trong ngày hôm nay
                        </td></tr>`
                    );
                }
            } else {
                overlay.style.display = 'none';
                notyf.error(data.message || 'Lưu thất bại!');
            }
        })
        .catch(() => {
            overlay.style.display = 'none';
            notyf.error('Có lỗi khi gửi dữ liệu!');
        });
});

$(document).on('click', '#closeTrackingModal', function () {
    $('#trackingModal').addClass('closing');
    setTimeout(function () {
        $('#trackingModal').addClass('hidden').removeClass('closing');
        // Reload lại trang sau khi modal đóng
        window.location.reload();
    }, 200); // Thời gian trùng với animation out
});

let currentEditId = null;
const currentEmployeeCode = '@ViewBag.CurrentEmployeeCode'.trim();

$(document).on('click', '.edit-btn', function () {
    // Lấy dữ liệu từ entityList (hoặc data attribute)
    const treatmentRecordStatus = $(this).data('treatmentrecord-status');
    if (treatmentRecordStatus == 2 || treatmentRecordStatus == 3) {
        notyf.error('Phiếu điều trị này đã hoàn thành hoặc đã hủy, không thể cập nhật!');
        return;
    }
    const id = $(this).data('id');
    const entity = window.entityList?.find(e => e.id === id);
    currentEditId = id;

    // Set tiêu đề phòng
    $('#editModalRoomTitle').text(entity ? `Ngày ${new Date(entity.trackingDate).toLocaleDateString('vi-VN')} – Phòng ${entity.roomName}` : '');

    // Set trạng thái, tên bệnh nhân, ghi chú
    $('#editModalPatientTableBody').empty();
    $('#editModalPatientTableBody').append(`<tr>
        <td style="font-weight:bold;">${entity ? entity.patientName : ''}</td>
        <td style="text-align:center;">
        <div class="btn-group d-flex justify-center" style="display:flex;gap:12px;justify-content:center;">
        <button class="btn btn-success btn-status${entity && entity.status == 1 ? ' active-status' : ''}" data-status="1">Có điều trị</button>
        <button class="btn btn-warning btn-status${entity && entity.status == 2 ? ' active-status' : ''}" data-status="2">Xin phép</button>
        <button class="btn btn-danger btn-status${entity && entity.status == 3 ? ' active-status' : ''}" data-status="3">Không điều trị</button>
        </div>
        </td>
        <td style="text-align:center;">
        <input type="text" class="form-control note-input" placeholder="Nhập ghi chú..." value="${entity && entity.note ? entity.note.replace(/\"/g, '&quot;') : ''}" style="width: 220px; margin: 0 auto;" />
        </td>
        <td style="text-align:center;">
        <button class="btn btn-primary btn-update">Cập nhật</button>
        </td>
        </tr>
    `);

    $('#editTrackingModal').removeClass('hidden closing');
});

// Chọn trạng thái
$('#editTrackingModal .btn-status').on('click', function () {
    $('#editTrackingModal .btn-status').removeClass('active-status');
    $(this).addClass('active-status');
});

// Đóng modal với hiệu ứng
$('#closeEditTrackingModal').on('click', function () {
    $('#editTrackingModal').addClass('closing');
    setTimeout(function () {
        $('#editTrackingModal').addClass('hidden').removeClass('closing');
        window.location.reload();
    }, 200);
});

// Submit cập nhật
$(document).on('click', '#editModalPatientTableBody .btn-update', function (e) {
    e.preventDefault();

    const $btn = $(this);
    if ($btn.prop('disabled')) return;

    $btn.prop('disabled', true);

    const $row = $(this).closest('tr');
    const status = $row.find('.btn-status.active-status').data('status');
    const note = $row.find('.note-input').val();
    const overlay = document.getElementById('loadingOverlay');

    if (!status) {
        notyf.error('Vui lòng chọn trạng thái!');
        return;
    }

    // --- VALIDATE 3 ngày liên tiếp không điều trị ---
    const entity = window.entityList?.find(e => e.id === currentEditId);
    if (entity) {
        const patientName = entity.patientName;
        // Lấy tất cả bản ghi của bệnh nhân này, sắp xếp theo ngày tăng dần
        const patientTrackings = window.entityList
            .filter(e => e.patientName === patientName)
            .sort((a, b) => new Date(a.trackingDate) - new Date(b.trackingDate));
        // Tạo bản sao và cập nhật trạng thái mới cho bản ghi đang sửa
        const updatedTrackings = patientTrackings.map(e => {
            if (e.id === currentEditId) {
                return { ...e, status: status };
            }
            return e;
        });
        // Duyệt toàn bộ danh sách, kiểm tra chuỗi 3 bản ghi liên tiếp 'Không điều trị'
        for (let i = 0; i < updatedTrackings.length - 3 + 1; i++) {
            if (
                updatedTrackings[i].status === 3 &&
                updatedTrackings[i + 1].status === 3 &&
                updatedTrackings[i + 2].status === 3
            ) {
                // Nếu sau chuỗi này còn bản ghi thì báo lỗi
                if (i + 3 < updatedTrackings.length) {
                    notyf.error('Bất thường: Có 3 ngày liên tiếp "Không điều trị" nhưng sau đó vẫn còn bản ghi theo dõi!');
                    $btn.prop('disabled', false);
                    return;
                }
            }
        }
    }
    // --- END VALIDATE ---

    overlay.style.display = 'flex';
    fetch('/api/trackinghandles/' + currentEditId, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ status, note })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                overlay.style.display = 'none';
                notyf.success('Cập nhật thành công!');
            } else {
                overlay.style.display = 'none';
                notyf.error(data.message || 'Cập nhật thất bại!');
            }
        })
        .catch(() => {
            overlay.style.display = 'none';
            notyf.error('Có lỗi khi gửi dữ liệu!');
        });
});

document.addEventListener('DOMContentLoaded', function () {
    // Lấy danh sách bệnh nhân duy nhất
    const entityList = window.entityList || [];
    const patientNames = [...new Set(entityList.map(e => e.patientName).filter(Boolean))];
    const patientSelect = document.getElementById('filterPatient');
    patientSelect.innerHTML = '<option value="">Chọn bệnh nhân...</option>' +
        patientNames.map(name => `<option value="${name}">${name}</option>`).join('');

    // Đổ option cho trạng thái
    const statusSelect = document.getElementById('filterStatus');
    const statusOptions = [
        { value: '', text: 'Chọn trạng thái...' },
        { value: 1, text: 'Có điều trị' },
        { value: 2, text: 'Xin phép' },
        { value: 3, text: 'Không điều trị' }
    ];
    statusSelect.innerHTML = statusOptions.map(opt => `<option value="${opt.value}">${opt.text}</option>`).join('');

    if (window.flatpickr) {
        flatpickr("#filterDate", {
            dateFormat: "d/m/Y",
            allowInput: true,
        });
        flatpickr("#filterDateFrom", {
            dateFormat: "d/m/Y",
            allowInput: true,
        });
        flatpickr("#filterDateTo", {
            dateFormat: "d/m/Y",
            allowInput: true,
        });
    }

    // Chỉ khởi tạo Choices.js sau khi đã đổ option xong và chưa có instance
    if (window.Choices) {
        if (!patientSelect.classList.contains('choices-initialized')) {
            window.patientChoices = new Choices(patientSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm...',
                removeItemButton: true,
                noResultsText: 'Không tìm thấy kết quả',
                noChoicesText: 'Không có lựa chọn nào',
                itemSelectText: ''
            });
            patientSelect.classList.add('choices-initialized');
        }
        if (!statusSelect.classList.contains('choices-initialized')) {
            window.statusChoices = new Choices(statusSelect, {
                searchEnabled: true,
                searchPlaceholderValue: 'Tìm kiếm...',
                removeItemButton: true,
                noResultsText: 'Không tìm thấy kết quả',
                noChoicesText: 'Không có lựa chọn nào',
                itemSelectText: ''
            });
            statusSelect.classList.add('choices-initialized');
        }
        setTimeout(function () {
            document.querySelectorAll('#filterPatient + .choices, #filterStatus + .choices').forEach(el => {
                el.style.width = '260px';
                el.style.minWidth = '260px';
                el.style.maxWidth = '260px';
                el.style.flex = 'none';
            });
            document.querySelectorAll('#filterPatient + .choices .choices__inner, #filterStatus + .choices .choices__inner').forEach(el => {
                el.style.width = '100%';
                el.style.minWidth = '100%';
                el.style.maxWidth = '100%';
            });
        }, 200);
    }
});
// Lọc danh sách khi nhấn nút Lọc
$(document).on('click', '#btnFilter', function () {
    const patient = $('#filterPatient').val();
    const date = $('#filterDate').val();
    const status = $('#filterStatus').val();
    const dateFrom = $('#filterDateFrom').val();
    const dateTo = $('#filterDateTo').val();

    // Nếu chỉ nhập 1 trong 2 trường khoảng ngày thì báo lỗi
    if ((dateFrom && !dateTo) || (!dateFrom && dateTo)) {
        notyf.error('Vui lòng chọn đầy đủ khoảng thời gian khi lọc nâng cao!');
        return;
    }

    // Nếu nhập đủ cả 2 trường thì kiểm tra logic ngày
    if (dateFrom && dateTo) {
        // Chuyển về dạng yyyy-mm-dd để so sánh
        const fromParts = dateFrom.split('/');
        const toParts = dateTo.split('/');
        const fromDate = new Date(`${fromParts[2]}-${fromParts[1]}-${fromParts[0]}`);
        const toDate = new Date(`${toParts[2]}-${toParts[1]}-${toParts[0]}`);
        if (fromDate > toDate) {
            notyf.error('Khoảng thời gian để lọc không hợp lệ!');
            return;
        }
    }

    // Nếu không có trường nào được chọn
    if (!patient && !date && !status && !dateFrom && !dateTo) {
        notyf.error('Cần chọn ít nhất một trường để lọc!');
        window.list.filteredList = window.list.entityList;
        if (window.list.datatable) window.list.datatable.destroy();
        window.list.initializeTable();
        updateResetFilterButtonState();
        return;
    }
    if (window.list && typeof window.list.filterList === 'function') {
        window.list.filterList(patient, date, status, dateFrom, dateTo);
    }
});

function updateResetFilterButtonState() {
    const patient = $('#filterPatient').val();
    const date = $('#filterDate').val();
    const status = $('#filterStatus').val();
    const dateFrom = $('#filterDateFrom').val();
    const dateTo = $('#filterDateTo').val();
    if (!patient && !date && !status && !dateFrom && !dateTo) {
        $('#btnResetFilter').prop('disabled', true).css('cursor', 'not-allowed');
    } else {
        $('#btnResetFilter').prop('disabled', false).css('cursor', 'pointer');
    }
}
$(document).ready(function () {
    updateResetFilterButtonState();
    $('#filterPatient, #filterDate, #filterStatus, #filterDateFrom, #filterDateTo').on('change input', function () {
        updateResetFilterButtonState();
        if (!$(this).val()) {
            window.list.filteredList = window.list.entityList;
            if (window.list.datatable) window.list.datatable.destroy();
            window.list.initializeTable();
        }
    });
});
$(document).on('click', '#btnResetFilter', function () {
    // Reset input ngày đơn
    $('#filterDate').val('');
    // Reset input khoảng ngày
    $('#filterDateFrom').val('');
    $('#filterDateTo').val('');

    // Reset Choices.js cho select bệnh nhân và trạng thái
    if (window.patientChoices) {
        window.patientChoices.removeActiveItems();
        window.patientChoices.setChoiceByValue('');
    } else {
        $('#filterPatient').val('').trigger('change');
    }
    if (window.statusChoices) {
        window.statusChoices.removeActiveItems();
        window.statusChoices.setChoiceByValue('');
    } else {
        $('#filterStatus').val('').trigger('change');
    }

    if (window.list && typeof window.list.filterList === 'function') {
        window.list.filterList('', '', '', '', '', false);
    }
    updateResetFilterButtonState();
});

$('#filterDate').on('change', function () {
    if ($(this).val()) {
        $('#filterDateFrom').val('');
        $('#filterDateTo').val('');
    }
});

function removeVietnameseTones(str) {
    if (typeof str !== 'string') return '';
    str = str.toLowerCase();
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    str = str.replace(/đ/g, "d");
    return str;
}

document.getElementById('customSearchInput').addEventListener('input', function () {
    const keyword = removeVietnameseTones(this.value.trim().toLowerCase());
    if (!window.list) return;

    try {
        if (keyword === '') {
            window.list.filteredList = [...window.list.entityList];
        } else {
            window.list.filteredList = window.list.entityList.filter(e => {
                // Chuẩn hóa tên bệnh nhân
                const patientName = removeVietnameseTones((e.patientName || '').toLowerCase());
                // Tách keyword thành các từ
                const keywordParts = keyword.split(' ').filter(Boolean);
                // Kiểm tra từng từ trong keyword có xuất hiện trong tên bệnh nhân không
                return keywordParts.every(part => patientName.includes(part))
                    || (e.code && e.code.toLowerCase().includes(keyword))
                    || (e.trackingDate && new Date(e.trackingDate).toLocaleDateString('vi-VN').includes(keyword))
                    || (e.status && getEnumDisplayName('TrackingStatus', e.status).toLowerCase().includes(keyword));
            });
        }
        window.list.updateTable();
    } catch (error) {
        console.error('Lỗi xử lý tìm kiếm:', error);
    }
});
