# TraditionalMedicineHospital
Đồ án tốt nghiệp K63 NTU - Xây dựng ứng dụng theo dõi điều trị của bệnh nhân điều trị ngoại trú tại Bệnh viện Y học cổ truyền Nha Trang

# Công nghệ ASP.NET CORE MVC + RAZOR
Phần mềm lập trình: Visual Studio 2022 (.NET 8.0) <br>
Hệ quản trị cơ sở dữ liệu: SQL Server 2019

# Đăng nhập trang quản lý hệ thống với vai trò quản trị viên
Tài khoản: admin <br>
Mật khẩu: 11111111

# Quy trình nghiệp vụ
1. Tiếp nhận bệnh nhân
- Đăng nhập với tư cách "Nhân viên hành chính" -> chức năng Tiếp nhận bệnh nhân để tiếp nhận bệnh nhân (Thông tin bệnh nhân + BHYT) -> Tài khoản + password của bệnh nhân sẽ được gửi vào email.
2. Lập phiếu khám bệnh
- Đăng nhập với tư cách "Bác sĩ" -> Quản lý phiếu điều trị -> Lập phiếu để thêm phiếu khám cho bệnh nhân.
3. Tạm ứng vào phiếu
- Đăng nhập với tư cách "Nhân viên hành chính" -> chức năng "Cập nhập tạm ứng" -> Chọn bệnh nhân + mã phiếu của bệnh nhân -> Nhập số tiền tạm ứng.
4. Theo dõi điều trị
- Đăng nhập với tư cách "Y tá" -> chức năng "Thêm bản ghi theo dõi" -> chọn trạng thái theo dõi của bệnh nhân -> Lưu.
5. Cảnh báo
- Trường hợp bệnh nhân có 2 bản ghi có trạng thái "Không điều trị" -> Cảnh báo vi phạm.
- Đăng nhập với tư cách "Nhân viên hành chính" -> Danh sách các bệnh nhân bị cảnh báo -> Chọn chức năng gửi cảnh báo qua email + sms đối với bệnh nhân bị cảnh báo.
- Trường hợp bệnh nhân có 3 bản ghi có trạng thái "Không điều trị" -> Đã vi phạm quy định.
- Đăng nhập với tư cách "Nhân viên hành chính" -> Danh sách các bệnh nhân bị vi phạm -> Chọn chức năng gửi tín hiệu đình chỉ phiếu đến bác sĩ.
6. Đình chỉ
- Đăng nhập với tư cách "Bác sĩ" -> Danh sách các phiếu điều trị -> Chức năng đình chỉ phiếu -> Chọn lý do phù hợp.
7. Thanh toán
- Đăng nhập với tư cách "Nhân viên hành chính" -> Quản lý thanh toán -> Lập phiếu thanh toán.
- Đăng nhập với tư cách "Bệnh nhân" -> Thanh toán phiếu thanh toán -> Xong quy trình khám chữa bệnh.
