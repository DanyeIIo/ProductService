DELETE FROM Products;
DELETE FROM GroupResults;

INSERT INTO Products (Name, Unit, PricePerUnit, Quantity)
VALUES
    ('Pencil', 'one', 1.5, 150),
    ('A4', 'bulk', 2.6, 50),
    ('Marker desk', 'one', 32, 11),
    ('100 papers', 'one', 10, 5);

    select * from products