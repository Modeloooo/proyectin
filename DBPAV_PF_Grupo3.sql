CREATE DATABASE DBPAV_PF3
GO

USE DBPAV_PF3
GO

CREATE TABLE Usuarios (
    Identificacion NVARCHAR(50) PRIMARY KEY NOT NULL,
    NombreCompleto NVARCHAR(150) NOT NULL,
    Genero NVARCHAR(20) NULL,
    Correo NVARCHAR(100) NOT NULL,
    TipoTarjeta NVARCHAR(20) NULL,
    NumeroTarjeta NVARCHAR(20) NULL,
    DineroDisponible DECIMAL(18,2) NOT NULL DEFAULT 0,
    Contrasena NVARCHAR(200) NOT NULL,
    Perfil NVARCHAR(15) NOT NULL DEFAULT 'Cliente',
	CodigoRecuperacion NVARCHAR(50) NULL,
    CodigoExpira DATETIME NULL
);

CREATE TABLE Categorias (
    CodigoCategoria INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    Descripcion NVARCHAR(100) NOT NULL
);

CREATE TABLE Productos (
    CodigoProducto INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- autogenerado
    Nombre NVARCHAR(150) NOT NULL,
    CodigoCategoria INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    RequiereEmpaque INT NOT NULL DEFAULT 0,
    CostoEmpaque DECIMAL(18,2) NOT NULL DEFAULT 0,
    Cantidad INT NOT NULL,
    Estado INT NOT NULL DEFAULT 1,
    UrlImagen NVARCHAR(300) NULL, -- nuevo campo para imagen
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CodigoCategoria) REFERENCES Categorias(CodigoCategoria)
);


CREATE TABLE Mesas (
    NumeroMesa INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- autogenerado
    Capacidad INT NOT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Libre'
);


CREATE TABLE Pedidos (
    CodigoPedido INT IDENTITY PRIMARY KEY NOT NULL,
    IdUsuario NVARCHAR(50) NOT NULL,
    TipoPedido NVARCHAR(20) NOT NULL,
    NumeroMesa INT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    Observaciones NVARCHAR(250) NULL,
    Total DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_Pedidos_Mesas FOREIGN KEY (NumeroMesa) REFERENCES Mesas(NumeroMesa),
    CONSTRAINT FK_Pedidos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Identificacion),
    CONSTRAINT CK_Pedidos_Estado CHECK (Estado IN ('Pendiente','Preparación','Servido','Cancelado'))
);

ALTER TABLE Pedidos
DROP CONSTRAINT CK_Pedidos_Estado;

ALTER TABLE Pedidos
ADD CONSTRAINT CK_Pedidos_Estado
CHECK (Estado IN ('Pendiente','Pagado','Cancelado','Preparación','Servido'));



CREATE TABLE PedidoDetalles (
    Id INT IDENTITY PRIMARY KEY NOT NULL,
    CodigoPedido INT NOT NULL,
    CodigoProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    CONSTRAINT FK_PedidoDetalle_Pedido FOREIGN KEY (CodigoPedido) REFERENCES Pedidos(CodigoPedido),
    CONSTRAINT FK_PedidoDetalle_Producto FOREIGN KEY (CodigoProducto) REFERENCES Productos(CodigoProducto)
);

INSERT INTO Usuarios (Identificacion, NombreCompleto, Genero, Correo, TipoTarjeta, NumeroTarjeta, DineroDisponible, Contrasena, Perfil)
VALUES 
('101', 'Carlos Ramírez', 'Masculino', 'carlos.admin@empresa.com', 'Visa', '4111111111111111', 5000.00, 'admin123', 'Administrador'),

('102', 'María López', 'Femenino', 'maria.cliente@correo.com', 'MasterCard', '5500000000000004', 1200.50, 'cliente123', 'Cliente'),

('103', 'José Fernández', 'Masculino', 'jose.salonero@empresa.com', 'Visa', '4111111111112222', 800.00, 'salonero123', 'Salonero'),

('104', 'Ana Rodríguez', 'Femenino', 'ana.cajera@empresa.com', 'MasterCard', '5500000000003333', 1500.75, 'cajero123', 'Cajero'),

('105', 'Luis Gómez', 'Masculino', 'luis.contador@empresa.com', 'Visa', '4111111111114444', 3000.00, 'contador123', 'Contador'),

('106', 'Sofía Herrera', 'Femenino', 'sofia.cocinera@empresa.com', 'MasterCard', '5500000000005555', 950.25, 'cocinero123', 'Cocinero');

INSERT INTO Categorias (Descripcion)
VALUES 
('Entradas Gourmet'),
('Platos Fuertes'),
('Postres');


INSERT INTO Productos (Nombre, CodigoCategoria, Precio, RequiereEmpaque, CostoEmpaque, Cantidad, Estado, UrlImagen)
VALUES
('Carpaccio de Res con Parmesano', 1, 18.50, 0, 0.00, 20, 1, 'https://miapp.com/img/carpaccio.jpg'),
('Filete Mignon con Salsa de Vino', 2, 42.00, 0, 0.00, 15, 1, 'https://miapp.com/img/filete-mignon.jpg'),
('Soufflé de Chocolate Belga', 3, 12.00, 0, 0.00, 25, 1, 'https://miapp.com/img/souffle-chocolate.jpg');


INSERT INTO Mesas (Capacidad, Estado)
VALUES
(2, 'Libre'),  
(4, 'Libre'),   
(6, 'Libre');


-- Pedido en mesa (cliente en mesa 1)
INSERT INTO Pedidos (IdUsuario, TipoPedido, NumeroMesa, Estado, Observaciones, Total)
VALUES ('102', 'Mesa', 1, 'Pendiente', 'Cliente pidió sin sal', 85.00);

-- Pedido delivery (sin mesa)
INSERT INTO Pedidos (IdUsuario, TipoPedido, NumeroMesa, Estado, Observaciones, Total)
VALUES ('102', 'Delivery', NULL, 'Preparación', 'Entrega en barrio Los Ángeles', 42.50);

-- Pedido en mesa cancelado (mesa 2)
INSERT INTO Pedidos (IdUsuario, TipoPedido, NumeroMesa, Estado, Observaciones, Total)
VALUES ('102', 'Mesa', 2, 'Cancelado', 'Cliente canceló por demora', 0.00);
