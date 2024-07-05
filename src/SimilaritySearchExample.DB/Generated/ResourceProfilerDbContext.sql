CREATE TABLE [Documents] (
    [Id] int NOT NULL IDENTITY,
    [FileName] nvarchar(max) NOT NULL,
    [Hash] nvarchar(max) NOT NULL,
    [ContentType] nvarchar(max) NOT NULL,
    [ContainerName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
);
GO


