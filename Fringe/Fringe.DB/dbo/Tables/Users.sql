CREATE TABLE [dbo].[Users] (
    [userid]    INT           IDENTITY (1, 1) NOT NULL,
    [firstname] VARCHAR (100) NOT NULL,
    [lastname]  VARCHAR (100) NOT NULL,
    [email]     VARCHAR (255) NOT NULL,
    [password]  VARCHAR (255) NOT NULL,
    [roleid]    INT           NOT NULL,
    [status]    TINYINT       DEFAULT ((1)) NULL,
    [createdid] INT           NOT NULL,
    [updatedid] INT           NOT NULL,
    [createdat] DATETIME      DEFAULT (getdate()) NULL,
    [updatedat] DATETIME      DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([userid] ASC),
    FOREIGN KEY ([roleid]) REFERENCES [dbo].[Roles] ([roleid]),
    UNIQUE NONCLUSTERED ([email] ASC)
);

