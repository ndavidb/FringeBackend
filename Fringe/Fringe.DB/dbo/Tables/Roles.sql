CREATE TABLE [dbo].[Roles] (
    [roleid]    INT           IDENTITY (1, 1) NOT NULL,
    [rolename]  VARCHAR (100) NOT NULL,
    [cancreate] BIT           DEFAULT ((0)) NULL,
    [canread]   BIT           DEFAULT ((0)) NULL,
    [canedit]   BIT           DEFAULT ((0)) NULL,
    [candelete] BIT           DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([roleid] ASC),
    UNIQUE NONCLUSTERED ([rolename] ASC)
);

