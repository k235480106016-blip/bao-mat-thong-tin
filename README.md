# Bài tập An toàn và Bảo mật thông tin: AES & RSA

Hai ứng dụng console viết bằng **C# / .NET** (Visual Studio 2022), thực hành mã hoá đối xứng AES, mã hoá bất đối xứng RSA, băm SHA-256 và chữ ký số.

> Sinh viên: **[Họ tên]** | MSSV: **[MSSV]** | Lớp: **[Lớp]**

## Cấu trúc repo

```
AES-RSA-BAITAP/
├── AesApp/     # Bài 1: AES
├── RsaApp/     # Bài 2: RSA, SHA-256, chữ ký số
├── images/     # Ảnh kết quả chạy
└── README.md
```

## Cách chạy

1. Mở file solution bằng Visual Studio 2022.
2. Chuột phải project muốn chạy, chọn **Set as Startup Project**.
3. Chọn cấu hình **Release**, nhấn **Ctrl+F5**.

---

# Bài 1: Mã hoá AES (đối xứng)

## Chức năng

| Menu | Chức năng |
|---|---|
| 1 | Sinh khoá AES (128/192/256 bit) |
| 2 | Lưu khoá ra file (base64) |
| 3 | Import khoá từ file |
| 4 | Mã hoá văn bản, in kết quả base64 ra màn hình |
| 5 | Giải mã văn bản |
| 6 | Mã hoá file, đo thời gian |
| 7 | Giải mã file, đo thời gian |
| 8 | So sánh 2 file theo từng byte |
| 9 | Tạo file test ngẫu nhiên theo dung lượng MB |

## Thiết kế

- AES-CBC, padding PKCS7, **IV ngẫu nhiên** mỗi lần mã hoá, ghi 16 byte IV ở đầu dữ liệu mã hoá.
- Xử lý file theo luồng (`CryptoStream`, buffer 1 MB) nên file hàng GB không tốn RAM.

## Kết quả

Cấu hình máy: **[CPU / RAM / SSD hay HDD]**. Khoá AES 256 bit, chế độ CBC, PKCS7.

### Sinh khoá và lưu khoá

Sinh khoá AES 256 bit và lưu ra file `E:\test\aes.key` (dạng base64).

![Sinh khoá và lưu khoá](images/aes_01_sinh_khoa.png)

### Mã hoá và giải mã văn bản

Mã hoá văn bản `diep` cho kết quả base64 `NcojddGycNRfjKUld9oc9u0fHDxPusWFFCU3zQX5tkQ=`. Dán chuỗi này vào chức năng giải mã thu lại đúng văn bản gốc `diep`. (Lần thử này dùng khoá 128 bit.)

![Mã hoá và giải mã văn bản](images/aes_02_van_ban.png)

### Mã hoá file 1 GB

Tạo file ngẫu nhiên `test1g.bin` dung lượng 1024 MB, rồi mã hoá thành `test1g.bin.enc`.

![Tạo file và mã hoá](images/aes_03_ma_hoa_file.png)

### Giải mã và so sánh byte

Giải mã `test1g.bin.enc` thành `test1g.bin.enc.dec`, rồi so sánh với file gốc theo từng byte.

![Giải mã và so sánh](images/aes_04_giai_ma_so_sanh.png)

| Dung lượng | Thời gian mã hoá | Tốc độ mã hoá | Thời gian giải mã | Tốc độ giải mã | So sánh byte |
|---|---|---|---|---|---|
| 1024 MB (1.073.741.824 byte) | 1,234 s | 830,1 MB/s | 0,631 s | 1622,4 MB/s | **Giống hệt** (0,393 s) |

### Nhận xét và kết luận Bài 1

- **Tính đúng đắn:** file sau giải mã có đúng 1.073.741.824 byte và **trùng khớp 100% từng byte** với file gốc, nên mã hoá rồi giải mã không làm mất hay sai lệch dữ liệu.
- **Hiệu năng:** AES xử lý 1 GB chỉ trong khoảng 1 giây (mã hoá 830 MB/s, giải mã 1622 MB/s). Điều này cho thấy AES (mã hoá đối xứng) rất nhanh, phù hợp mã hoá dữ liệu lớn.
- **Giải mã nhanh hơn mã hoá khoảng 2 lần (1,234 s so với 0,631 s):** trong chế độ CBC, mỗi khối mã hoá phụ thuộc kết quả của khối trước nên phải xử lý tuần tự. Khi giải mã, các khối độc lập nhau nên CPU (có tập lệnh AES-NI) có thể xử lý song song và nhanh hơn. Ngoài ra lần chạy này chỉ đo một lần, nên còn có thể chịu ảnh hưởng của bộ nhớ đệm file và tốc độ ghi ổ đĩa; muốn kết luận chắc hơn cần chạy lặp lại nhiều lần.
- **Bộ nhớ:** chương trình đọc và ghi theo từng khối 1 MB (streaming) nên không cần nạp cả file hàng GB vào RAM.
- **Khoá:** mã hoá và giải mã dùng chung một khoá, nên nếu mất file khoá `aes.key` thì không thể giải mã dữ liệu.

---

# Bài 2: RSA, SHA-256, chữ ký số

## Chức năng

| Menu | Chức năng |
|---|---|
| 1 | Sinh cặp khoá RSA 2048 bit |
| 2 | Xuất 4 file PEM: `public.pem`, `private.pem`, `public_pkcs1.pem`, `private_pkcs1.pem` |
| 3 | Xem PEM và giải thích cấu trúc ASN.1/DER |
| 4 | Import PEM, hiển thị n, e (và d, p, q, dp, dq, qInv với private key) |
| 5 | Mã hoá RSA-OAEP-SHA256, lưu base64 |
| 6 | Giải mã và so sánh với dữ liệu gốc |
| 7 | Đánh giá tốc độ public key và private key |
| 8 | Băm SHA-256, đánh giá hiệu ứng thác đổ |
| 9 | Chữ ký số: PKCS#1 v1.5 và PSS / nonce |

## 1. Sinh cặp khoá và xuất PEM

Sinh khoá RSA 2048 bit, xuất ra 4 file PEM tại thư mục `bin\Release\net10.0`.

![Sinh khoá và xuất PEM](images/rsa_01_sinh_khoa.png)

## 2. Cấu trúc file PEM

File `private.pem` có dòng đầu `-----BEGIN PRIVATE KEY-----` (định dạng PKCS#8). Phần giữa là base64 của **1217 byte DER**. Dữ liệu DER bắt đầu bằng `30 82 04 BD ...`.

Mỗi phần tử DER gồm **Tag | Length | Value**:

| Tag | Ý nghĩa |
|---|---|
| `30` | SEQUENCE |
| `02` | INTEGER |
| `03` | BIT STRING |
| `04` | OCTET STRING |
| `05` | NULL |
| `06` | OBJECT IDENTIFIER |

Phân tích byte đầu `30 82 04 BD`:

- `30`: SEQUENCE.
- `82 04 BD`: độ dài 2 byte, `0x04BD` = 1213 byte (cộng 4 byte tiêu đề thành 1217 byte DER).

Cấu trúc của `private.pem` (kết quả thật từ chương trình):

```
[30] SEQUENCE, len=1213
  [02] INTEGER, len=1                    = 00            (version)
  [30] SEQUENCE, len=13
    [06] OID, len=9                      = 1.2.840.113549.1.1.1 (rsaEncryption)
    [05] NULL
  [04] OCTET STRING, len=1191            (chứa khoá RSA dạng PKCS#1)
    [30] SEQUENCE, len=1187
      [02] INTEGER, len=1   = 00         (version)
      [02] INTEGER, len=257 = 00-D4-67...  n (có byte 00 đứng đầu)
      [02] INTEGER, len=3   = 01-00-01     e = 65537
      [02] INTEGER, len=257 = 00-9F-0D...  d
      [02] INTEGER, len=129 = 00-DC-BB...  p
      [02] INTEGER, len=129 = 00-F6-57...  q
      [02] INTEGER, len=128 = 45-77-34...  d mod (p-1)
      [02] INTEGER, len=128 = 2C-7B-67...  d mod (q-1)
      [02] INTEGER, len=128 = 58-01-7B...  q^-1 mod p
```

Ghi chú: INTEGER có byte `00` đứng đầu khi bit cao của byte đầu bằng 1, để số luôn được hiểu là số dương. Vì vậy n có 257 byte dù khoá chỉ 2048 bit (256 byte).

![Xem file private.pem](images/rsa_02_xem_pem.png)

## 3. Import PEM

Import `private.pem` thu được:

| Thành phần | Kích thước | Giá trị |
|---|---|---|
| n (Modulus) | 2048 bit (256 byte) | 617 chữ số thập phân |
| e (PublicExponent) | 17 bit | 65537 (hex `010001`) |
| d (PrivateExponent) | 2048 bit | 617 chữ số thập phân |
| p, q | 1024 bit | 309 chữ số thập phân |
| dp, dq, qInv | 1024 bit | 308 chữ số thập phân |

Lưu ý: menu 4 hiển thị e là "24 bit" vì tính theo độ dài mảng byte (3 byte). Giá trị thật là 65537, chỉ 17 bit.

![Import private.pem](images/rsa_03_import_pem.png)

## 4. Mã hoá và giải mã

- Dùng RSA-OAEP-SHA256 với khoá 2048 bit. Dữ liệu tối đa 256 − 2×32 − 2 = **190 byte**.
- Dữ liệu thử: 32 byte ngẫu nhiên (cỡ khoá AES-256). Kết quả mã hoá dài 256 byte, lưu base64 vào `cipher.b64`.
- Giải mã bằng `private.pem` thu lại 32 byte, chương trình báo **"GIỐNG dữ liệu gốc"**.

![Mã hoá và giải mã RSA](images/rsa_04_ma_hoa_giai_ma.png)

## 5. Đánh giá tốc độ: public key (e nhỏ) và private key (d lớn)

| Độ dài khoá | e (bit) | d (bit) | Mã hoá, public (ms) | Giải mã, private (ms) | Dec/Enc | Verify (ms) | Sign (ms) |
|---|---|---|---|---|---|---|---|
| 1024 | 17 | 1020 | 0.008 | 0.213 | 26.7x | 0.011 | 0.149 |
| 2048 | 17 | 2047 | 0.030 | 0.866 | 29.3x | 0.024 | 0.804 |
| 3072 | 17 | 3070 | 0.079 | 2.841 | 35.8x | 0.050 | 3.141 |
| 4096 | 17 | 4093 | 0.087 | 6.398 | 73.9x | 0.091 | 6.358 |

**Nhận xét:**

- Phép toán với khoá công khai (e = 65537, chỉ 17 bit) rất nhanh. Phép toán với khoá bí mật (d cỡ bằng n) chậm hơn từ **26 đến 74 lần**.
- Khi tăng độ dài khoá, thời gian dùng khoá bí mật tăng mạnh (khoá 2048 bit là 0.866 ms, 4096 bit là 6.398 ms, tăng khoảng 7.4 lần), còn thời gian dùng khoá công khai tăng chậm hơn nhiều vì e luôn là 17 bit. Do đó tỉ lệ Dec/Enc càng lớn khi khoá càng dài.
- Ký (dùng private key) chậm hơn xác minh (dùng public key) tương tự. Điều này hợp lý vì một bên ký thì nhiều bên xác minh.

![Bảng tốc độ RSA](images/rsa_05_toc_do.png)

## 6. Băm SHA-256 và hiệu ứng thác đổ

| Đầu vào 1 | Đầu vào 2 | Số bit khác | Tỉ lệ | Số byte khác |
|---|---|---|---|---|
| `Hello, Hellp, hello` | `Hellp` | 129/256 | 50.4% | 32/32 |
| `Hellp` | `Hello` | 127/256 | 49.6% | 32/32 |

SHA-256 của `Hellp`: `2113cd4a7aebc3a400a234c3de8a5c0c4d9fa8c6ca370d030aac9ab51e8c4476`

SHA-256 của `Hello`: `185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969`

**Nhận xét:** hai chuỗi `Hellp` và `Hello` chỉ khác đúng 1 ký tự nhưng kết quả băm khác **127/256 bit (khoảng 50%)** và cả 32 byte đều đổi. Đây là **hiệu ứng thác đổ (avalanche effect)**: đầu vào thay đổi rất ít thì kết quả băm thay đổi hoàn toàn, không thể đoán trước, nên không suy ngược được đầu vào từ giá trị băm.

![SHA-256](images/rsa_06_sha256.png)

## 7. Chữ ký số và đề xuất để chữ ký khác nhau mỗi lần ký

Chữ ký số là RSA dùng **khoá bí mật** trên giá trị băm của dữ liệu. Yêu cầu: cùng dữ liệu, cùng người ký (cùng private key) nhưng mỗi lần ký cho chữ ký khác nhau.

**Vấn đề:** RSA PKCS#1 v1.5 là **tất định**, nên cùng dữ liệu và cùng khoá luôn cho cùng chữ ký.

**Đề xuất:**

1. **RSASSA-PSS (chuẩn, nên dùng):** khi ký, thuật toán trộn thêm một **salt ngẫu nhiên** vào dữ liệu. Salt được nhúng trong chữ ký nên người nhận vẫn xác minh được bằng public key. Trong .NET dùng `RSASignaturePadding.Pss`.
2. **Thêm nonce:** ký H(dữ liệu ‖ nonce) với nonce ngẫu nhiên, gửi kèm nonce cùng chữ ký.

**Kết quả thử (dữ liệu `Hello`, cùng `private.pem`):**

| Cách ký | Hai lần ký giống nhau? | Verify |
|---|---|---|
| [A] PKCS#1 v1.5 | **Giống nhau (True)** | |
| [B] RSASSA-PSS | **Khác nhau (False)** | Cả hai lần đều True |
| [C] Nonce | **Khác nhau** | Cả hai lần đều True |

Hai chữ ký PSS của cùng dữ liệu `Hello`:

- Lần 1: `RQIsD6l6kpux4gUyDgtalxiaii9JjWkV8BohlbxxmgnAOB0t...`
- Lần 2: `F9XILBjHcg3laqptgK0GvBg0Uz0zoM5xvskpYOUiPOheFn+s...`

Như vậy PSS đạt được mong muốn của đề: cùng dữ liệu và cùng người ký nhưng chữ ký khác nhau ở mỗi lần ký, và tất cả đều xác minh đúng.

![Chữ ký số](images/rsa_07_chu_ky_so.png)

---

## Kết luận chung

- **AES** (đối xứng) dùng chung một khoá cho mã hoá và giải mã, rất nhanh (khoảng 830 MB/s mã hoá và 1622 MB/s giải mã trên máy thử), phù hợp để mã hoá dữ liệu lớn. File giải mã trùng khớp từng byte với file gốc.
- **RSA** (bất đối xứng) dùng cặp khoá, chỉ mã hoá được dữ liệu nhỏ (tối đa 190 byte với khoá 2048 bit), nên thường dùng để mã hoá khoá AES 32 byte, và chậm hơn AES nhiều. Dùng khoá bí mật chậm hơn khoá công khai hàng chục lần.
- **SHA-256** có hiệu ứng thác đổ: đổi 1 ký tự thì khoảng 50% bit kết quả thay đổi.
- **Chữ ký số** dùng RSASSA-PSS cho phép cùng dữ liệu, cùng người ký, chữ ký mỗi lần một khác mà vẫn xác minh được.

## Tài liệu tham khảo

- Microsoft Docs: `System.Security.Cryptography` (Aes, RSA, SHA256)
- RFC 8017 (PKCS#1: RSA-OAEP, RSA-PSS), RFC 5208 (PKCS#8)
