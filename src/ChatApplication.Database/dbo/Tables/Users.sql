CREATE TABLE [dbo].[Users]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [UserName] NVARCHAR(256) NOT NULL,
    [NormalizedUserName] NVARCHAR(256) NOT NULL,
    [Email] NVARCHAR(256) NOT NULL,
    [NormalizedEmail] NVARCHAR(256) NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,

    PRIMARY KEY([Id])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserName]
ON [dbo].[Users]([NormalizedUserName]);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Email]
ON [dbo].[Users]([NormalizedEmail]);