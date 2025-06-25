ALTER TABLE Tickets
ADD TicketPriceId INT NULL;

-- Add Quantity column as nullable
ALTER TABLE Tickets
ADD Quantity INT NULL;

-- Add Price column as nullable
ALTER TABLE Tickets
ADD Price DECIMAL(18,2) NULL;