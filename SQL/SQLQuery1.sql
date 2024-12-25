CREATE TABLE Styles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL, -- Tên phong cách

    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) DEFAULT 'Admin',
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(50) DEFAULT 'Admin'
);

INSERT INTO Styles (Name)
VALUES
(N'Dáng trực'),
(N'Dáng nghiêng'),
(N'Dáng làng'),
(N'Dáng huyền');

CREATE TABLE GeneralMeanings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Meaning NVARCHAR(100) NOT NULL, -- Ý nghĩa phong thủy

    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) DEFAULT 'Admin',
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(50) DEFAULT 'Admin'
);
INSERT INTO GeneralMeanings (Meaning)
VALUES
(N'Trường tồn'),
(N'Phú quý'),
(N'Hòa bình'),
(N'Thịnh vượng'),
(N'Sung túc'),
(N'Kiên nhẫn'),
(N'Sắc đẹp'),
(N'Bình yên'),
(N'Trong sáng'),
(N'Hạnh phúc');

CREATE TABLE Types (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL, -- Loại cây
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) DEFAULT 'Admin',
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(50) DEFAULT 'Admin'
);
INSERT INTO Types (Name)
VALUES
(N'Cây lá kim'),
(N'Cây lá rộng'),
(N'Cây hoa'),
(N'Cây ăn quả');

CREATE TABLE AdminUsers (
   USE_ID INT PRIMARY KEY IDENTITY (1,1),
   Username NVARCHAR(50), --Tên dùng để đăng nhập không thể bị trùng và thay đổi
   Avatar NVARCHAR(1000),
   Password NVARCHAR(12),
   Displayname NVARCHAR(50),-- Tên dùng để hiển thị
   Address NVARCHAR(100),
   Email NVARCHAR(50),
   Phone NVARCHAR(10),
   Role NVARCHAR(10),
);

INSERT INTO AdminUsers (Username,Avatar,Password,Displayname,Email,Phone,Role,Address)
VALUES
(N'Admin','','Admin123','Admin','','','Admin',N'');

CREATE TABLE Bonsais (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BonsaiName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX), -- Mô tả
    FengShuiMeaning NVARCHAR(100),
    Size INT, -- Kích thước (cm)
    YearOld INT , -- Tuổi hiện tại
    MinLife INT, -- Vòng đời thấp nhất
    MaxLife INT, -- Vòng đời cao nhất
    Price DECIMAL,
    Quantity INT,
    Image NVARCHAR(1000),
    NOPWR FLOAT DEFAULT 0, -- Số người đánh giá, mặc định là 0
    Rates INT DEFAULT 0, -- Số sao trung bình, mặc định là 0
    TypeId INT, -- FK đến bảng Types
    GeneralMeaningId INT, -- FK đến bảng FengShuis
    StyleId INT, -- FK đến bảng Styles

    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) DEFAULT 'Admin',
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(50) DEFAULT 'Admin'

    FOREIGN KEY (TypeId) REFERENCES Types(Id),
    FOREIGN KEY (GeneralMeaningId) REFERENCES GeneralMeanings(Id),
    FOREIGN KEY (StyleId) REFERENCES Styles(Id)
);

INSERT INTO Bonsais (BonsaiName, Description, FengShuiMeaning,Price , Quantity, Size, YearOld, MinLife, MaxLife, TypeId, GeneralMeaningId, StyleId,Image)
VALUES
-- Cây lá kim
(N'Cây tùng', N'Biểu tượng của sự trường thọ, thích hợp làm cây cảnh và bonsai.', N'Tượng trưng cho sự trường tồn và thịnh vượng.',100,10, 20, 10, 20, 100, 1, 3, 1,''),
-- Cây lá rộng
(N'Cây đa cảnh', N'Thể hiện sự trường thọ, tạo điểm nhấn cho không gian xanh.', N'Tượng trưng cho sự trường tồn và che chở.',100,10, 50, 15, 50, 200, 2, 1, 2,''),
(N'Cây si và sanh', N'Có bộ rễ đẹp, thường được tạo dáng bonsai nghệ thuật.', N'Tượng trưng cho sự bền bỉ và sức sống mạnh mẽ.',100,10, 40, 12, 30, 150, 2, 6, 3,''),
(N'Cây phong lá đỏ', N'Đẹp mắt với màu lá đỏ rực, tạo nét nổi bật cho sân vườn.', N'Tượng trưng cho sự may mắn và sắc đẹp.',100,10, 35, 8, 15, 50, 2, 2, 4,''),
(N'Cây nguyệt quế', N'Mang lại sự may mắn và tài lộc, dễ chăm sóc.', N'Tượng trưng cho sự may mắn và sắc đẹp.',100,10, 30, 10, 10, 30, 2, 7, 1,''),
(N'Cây duối cảnh', N'Thường dùng để tạo bonsai, biểu tượng của sự kiên nhẫn.', N'Tượng trưng cho sự gắn kết và trường tồn.',100,10, 30, 12, 20, 100, 2, 1, 3,''),
-- Cây hoa
(N'Cây hoa giấy', N'Mang màu sắc rực rỡ, phù hợp trang trí sân vườn hoặc ban công.', N'Tượng trưng cho sự giản dị và bền bỉ.',100,10, 25, 5, 10, 30, 3, 7, 1,''),
(N'Cây mẫu đơn đỏ', N'Biểu tượng của sự phú quý, thích hợp làm cây cảnh trang trí.', N'Tượng trưng cho sự giàu sang và phú quý.',100,10, 30, 6, 15, 50, 3, 4, 2,''),
(N'Cây mai chiếu thủy', N'Nhỏ nhắn, hoa thơm ngát, thường được uốn bonsai.', N'Tượng trưng cho sự bình yên và tinh khiết.',100,10, 15, 3, 10, 40, 3, 8, 3,''),
(N'Cây hoa nhài', N'Hoa trắng tinh khiết, có hương thơm dễ chịu.', N'Tượng trưng cho sự ngây thơ và trong sáng.',100,10, 15, 10, 10, 20, 3, 9, 1,''),
(N'Cây hoa đỗ quyên', N'Màu sắc đa dạng, thích hợp cho sân vườn và lối đi.', N'Tượng trưng cho sự sung túc và hạnh phúc.',100,10, 35, 8, 10, 30, 3, 10, 2,''),
-- Cây ăn quả
(N'Cây sung cảnh', N'Biểu tượng của sự sung túc, thường dùng làm bonsai.', N'Tượng trưng cho sự giàu sang và sung túc.',100,10, 30, 10, 25, 80, 4, 5, 2,''),
(N'Cây khế cảnh', N'Tượng trưng cho sự tài lộc, thích hợp làm bonsai sân vườn.', N'Tượng trưng cho sự tài lộc và bình yên.',100,10, 40, 12, 30, 100, 4, 2, 4,'');

CREATE TABLE Carts (
    CART_ID INT PRIMARY KEY IDENTITY(1,1),
    USE_ID INT,

    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) DEFAULT 'Admin',
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(50) DEFAULT 'Admin'

    FOREIGN KEY (USE_ID) REFERENCES AdminUsers(USE_ID),
);

CREATE TABLE CartDetails (
    CART_D_ID INT PRIMARY KEY IDENTITY(1,1),
    CART_ID INT,
    BONSAI_ID INT,
    QUANTITY INT,
    PRICE DECIMAL,
    TotalPrice AS (QUANTITY * PRICE),
    
    FOREIGN KEY (CART_ID) REFERENCES Carts(CART_ID),
    FOREIGN KEY (BONSAI_ID) REFERENCES Bonsais(ID)
);

	CREATE TABLE Orders (
		ORDER_ID INT PRIMARY KEY IDENTITY(1,1),
		USE_ID INT,
		Paymentmethod NVARCHAR(100),
		Status NVARCHAR(24),
		Total DECIMAL,

		CreatedDate DATETIME DEFAULT GETDATE(),
		CreatedBy NVARCHAR(50) DEFAULT 'Admin',
		UpdatedDate DATETIME DEFAULT GETDATE(),
		UpdatedBy NVARCHAR(50) DEFAULT 'Admin'

		FOREIGN KEY (USE_ID) REFERENCES AdminUsers(USE_ID),
	);

CREATE TABLE OrderDetails (
    ORDER_D_ID INT PRIMARY KEY IDENTITY(1,1),
    ORDER_ID INT,
    BONSAI_ID INT,
    QUANTITY INT,
    PRICE DECIMAL,
    TotalPrice AS (QUANTITY * PRICE),
    
    FOREIGN KEY (ORDER_ID) REFERENCES Orders(ORDER_ID),
    FOREIGN KEY (BONSAI_ID) REFERENCES Bonsais(ID)
);

CREATE TABLE Reviews (
    REVIEW_ID INT PRIMARY KEY IDENTITY(1,1),
    BONSAI_ID INT,
    USE_ID INT,
    Rate FLOAT, 
	Comment NVARCHAR(2048),
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) DEFAULT 'Admin',
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(50) DEFAULT 'Admin',
    
    FOREIGN KEY (USE_ID) REFERENCES AdminUsers(USE_ID),
    FOREIGN KEY (BONSAI_ID) REFERENCES Bonsais(ID)
);

CREATE VIEW BonsaiPhanLoai AS
SELECT 
    b.Id AS BonsaiId, 
    b.BonsaiName AS BonsaiName, 
    b.Description,
    b.FengShuiMeaning,
    b.Image,
    b.NOPWR,
    b.Rates,
    b.Size, 
    b.YearOld, 
    b.MinLife, 
    b.MaxLife,
    t.Name AS TypeName, 
    g.Meaning AS FengShui, 
    s.Name AS StyleName
FROM 
    Bonsais b
JOIN 
    Types t ON b.TypeId = t.Id
JOIN 
    GeneralMeanings g ON b.GeneralMeaningId = g.Id
JOIN 
    Styles s ON b.StyleId = s.Id;

 CREATE TABLE Banners (
    BAN_ID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255),
    ImageURL NVARCHAR(1024)
);


DROP TABLE Bonsais;
DROP TABLE GeneralMeanings;
DROP TABLE Banners;
DROP TABLE Styles;
DROP TABLE Types;
DROP TABLE Carts;
DROP TABLE CartDetails;
DROP TABLE Orders;
DROP TABLE OrderDetails;
DROP TABLE Reviews;
DROP TABLE AdminUsers;
DROP VIEW BonsaiPhanLoai;

SELECT *FROM GeneralMeanings;
SELECT *FROM Banners;
SELECT *FROM Styles;
SELECT *FROM Types;
SELECT *FROM Carts;
SELECT *FROM CartDetails;
SELECT *FROM Orders;
SELECT *FROM OrderDetails;
SELECT *FROM Reviews;
SELECT *FROM AdminUsers;
Select * FROM Bonsais
Select * FROM BonsaiPhanLoai