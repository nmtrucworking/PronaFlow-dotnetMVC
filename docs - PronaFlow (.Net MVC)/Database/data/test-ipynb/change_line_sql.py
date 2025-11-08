def chia_cac_lenh_insert_theo_vi_tri(duong_dan_file_goc, duong_dan_file_moi, cac_khoi_insert, so_ban_ghi_toi_da=1000):
    """
    Tách các câu lệnh INSERT lớn trong tệp SQL dựa trên vị trí dòng được chỉ định.

    Args:
        duong_dan_file_goc (str): Đường dẫn đến tệp SQL đầu vào.
        duong_dan_file_moi (str): Đường dẫn để lưu tệp SQL đã chỉnh sửa.
        cac_khoi_insert (list): Danh sách các khối insert, mỗi khối là một list 
                                [dòng_lệnh_insert, dòng_value_đầu_tiên, dòng_value_cuối_cùng].
        so_ban_ghi_toi_da (int): Số lượng bản ghi tối đa cho mỗi câu lệnh INSERT.
    """
    try:
        # Chuyển đổi vị trí dòng từ 1-based (người dùng cung cấp) sang 0-based (Python sử dụng)
        cac_khoi_0_based = [[vi_tri - 1 for vi_tri in khoi] for khoi in cac_khoi_insert]
        
        # Tạo một map để tra cứu thông tin khối dựa trên dòng bắt đầu
        map_tra_cuu_khoi = {khoi[0]: khoi for khoi in cac_khoi_0_based}

        with open(duong_dan_file_goc, 'r', encoding='utf-8') as f_goc:
            tat_ca_cac_dong = f_goc.readlines()

        cac_dong_moi = []
        i = 0
        tong_so_dong = len(tat_ca_cac_dong)

        while i < tong_so_dong:
            if i in map_tra_cuu_khoi:
                # Nếu dòng hiện tại là điểm bắt đầu của một khối INSERT lớn cần xử lý
                _, vi_tri_value_dau, vi_tri_value_cuoi = map_tra_cuu_khoi[i]
                
                # Trích xuất phần header (từ 'INSERT INTO' đến trước dòng value đầu tiên)
                cac_dong_header = tat_ca_cac_dong[i : vi_tri_value_dau]
                
                # Trích xuất các dòng chứa giá trị (values)
                cac_dong_gia_tri = tat_ca_cac_dong[vi_tri_value_dau : vi_tri_value_cuoi + 1]

                # Bắt đầu chia các dòng giá trị thành các đoạn nhỏ (chunk)
                for j in range(0, len(cac_dong_gia_tri), so_ban_ghi_toi_da):
                    doan_nho = cac_dong_gia_tri[j : j + so_ban_ghi_toi_da]

                    # Thêm header cho câu lệnh INSERT mới
                    cac_dong_moi.extend(cac_dong_header)

                    # Thêm các dòng giá trị cho đoạn nhỏ này
                    for k, dong in enumerate(doan_nho):
                        if k == len(doan_nho) - 1:  # Đây là dòng cuối cùng trong đoạn nhỏ
                            # Xóa dấu phẩy ',' hoặc ký tự xuống dòng ở cuối, sau đó thêm dấu ';'
                            cac_dong_moi.append(dong.rstrip(',\n') + ';\n')
                        else:
                            cac_dong_moi.append(dong)

                    # Thêm câu lệnh GO để kết thúc một lô (batch)
                    cac_dong_moi.append("GO\n")
                
                # Di chuyển con trỏ 'i' đến sau khối vừa xử lý
                i = vi_tri_value_cuoi + 1
                # Bỏ qua cả dòng 'GO' gốc của khối đó nếu có
                if i < tong_so_dong and tat_ca_cac_dong[i].strip().upper() == 'GO':
                    i += 1
            else:
                # Nếu dòng này không thuộc khối nào cần xử lý, sao chép nó như cũ
                cac_dong_moi.append(tat_ca_cac_dong[i])
                i += 1

        with open(duong_dan_file_moi, 'w', encoding='utf-8') as f_moi:
            f_moi.writelines(cac_dong_moi)

        print(f"Đã xử lý xong! Tệp mới đã được lưu tại: {duong_dan_file_moi}")

    except FileNotFoundError:
        print(f"Lỗi: Không tìm thấy tệp '{duong_dan_file_goc}'")
    except Exception as e:
        print(f"Đã xảy ra một lỗi không mong muốn: {e}")


# --- Cách sử dụng hàm ---

# 1. Dữ liệu vị trí dòng bạn đã cung cấp
vi_tri_cac_lenh_insert = [
    [338, 339, 1839],
    [1846, 1847, 5667],
    [5668, 5669, 18885],
    [18888, 18889, 24930],
    [24937, 24938, 34448],
    [34450, 34451, 133466],
    [133468, 133469, 319934],
    [319935, 319936, 438894],
    [438896, 438897, 468350],
    [468352, 468353, 488173]
]

# 2. Tên tệp SQL gốc và tên tệp bạn muốn lưu kết quả
input_file = 'PronaFlow_New_Sample_Data_EN.sql'
output_file = 'PronaFlow_New_Sample_Data_EN_split_by_lines.sql'

# 3. Gọi hàm để thực hiện xử lý
chia_cac_lenh_insert_theo_vi_tri(input_file, output_file, vi_tri_cac_lenh_insert, so_ban_ghi_toi_da=1000)