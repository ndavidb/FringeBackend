create table AgeRestrictionLookup
(
    agerestrictionid int identity
        constraint PK_AgeRestrictionLookup
            primary key,
    code             nvarchar(10)  not null,
    description      nvarchar(200) not null
)
    go

create table AspNetRoles
(
    Id               uniqueidentifier not null
        constraint PK_AspNetRoles
            primary key,
    Name             nvarchar(256),
    NormalizedName   nvarchar(256),
    ConcurrencyStamp nvarchar(max)
)
    go

create table AspNetRoleClaims
(
    Id         int identity
        constraint PK_AspNetRoleClaims
            primary key,
    RoleId     uniqueidentifier not null
        constraint FK_AspNetRoleClaims_AspNetRoles_RoleId
            references AspNetRoles
            on delete cascade,
    ClaimType  nvarchar(max),
    ClaimValue nvarchar(max)
)
    go

create index IX_AspNetRoleClaims_RoleId
    on AspNetRoleClaims (RoleId)
    go

create unique index RoleNameIndex
    on AspNetRoles (NormalizedName)
    where [NormalizedName] IS NOT NULL
go

create table AspNetUsers
(
    Id                   uniqueidentifier not null
        constraint PK_AspNetUsers
            primary key,
    FirstName            nvarchar(max)    not null,
    LastName             nvarchar(max)    not null,
    CreatedAt            datetime2        not null,
    IsActive             bit              not null,
    UserName             nvarchar(256),
    NormalizedUserName   nvarchar(256),
    Email                nvarchar(256),
    NormalizedEmail      nvarchar(256),
    EmailConfirmed       bit              not null,
    PasswordHash         nvarchar(max),
    SecurityStamp        nvarchar(max),
    ConcurrencyStamp     nvarchar(max),
    PhoneNumber          nvarchar(max),
    PhoneNumberConfirmed bit              not null,
    TwoFactorEnabled     bit              not null,
    LockoutEnd           datetimeoffset,
    LockoutEnabled       bit              not null,
    AccessFailedCount    int              not null
)
    go

create table AspNetUserClaims
(
    Id         int identity
        constraint PK_AspNetUserClaims
            primary key,
    UserId     uniqueidentifier not null
        constraint FK_AspNetUserClaims_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    ClaimType  nvarchar(max),
    ClaimValue nvarchar(max)
)
    go

create index IX_AspNetUserClaims_UserId
    on AspNetUserClaims (UserId)
    go

create table AspNetUserLogins
(
    LoginProvider       nvarchar(450)    not null,
    ProviderKey         nvarchar(450)    not null,
    ProviderDisplayName nvarchar(max),
    UserId              uniqueidentifier not null
        constraint FK_AspNetUserLogins_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    constraint PK_AspNetUserLogins
        primary key (LoginProvider, ProviderKey)
)
    go

create index IX_AspNetUserLogins_UserId
    on AspNetUserLogins (UserId)
    go

create table AspNetUserRoles
(
    UserId uniqueidentifier not null
        constraint FK_AspNetUserRoles_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    RoleId uniqueidentifier not null
        constraint FK_AspNetUserRoles_AspNetRoles_RoleId
            references AspNetRoles
            on delete cascade,
    constraint PK_AspNetUserRoles
        primary key (UserId, RoleId)
)
    go

create index IX_AspNetUserRoles_RoleId
    on AspNetUserRoles (RoleId)
    go

create table AspNetUserTokens
(
    UserId        uniqueidentifier not null
        constraint FK_AspNetUserTokens_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    LoginProvider nvarchar(450)    not null,
    Name          nvarchar(450)    not null,
    Value         nvarchar(max),
    constraint PK_AspNetUserTokens
        primary key (UserId, LoginProvider, Name)
)
    go

create index EmailIndex
    on AspNetUsers (NormalizedEmail)
    go

create unique index UserNameIndex
    on AspNetUsers (NormalizedUserName)
    where [NormalizedUserName] IS NOT NULL
go

create table Locations
(
    locationid       int identity
        constraint PK_Locations
            primary key,
    locationname     nvarchar(100)                 not null,
    address          nvarchar(max)                 not null,
    suburb           nvarchar(max)                 not null,
    postalcode       nvarchar(max)                 not null,
    state            nvarchar(max)                 not null,
    country          nvarchar(max)                 not null,
    latitude         float                         not null,
    longitude        float                         not null,
    parkingavailable bit default CONVERT([bit], 0) not null,
    active           bit default CONVERT([bit], 1) not null,
    createdbyid      int                           not null,
    createdat        datetime2                     not null,
    updatedid        int,
    updatedat        datetime2
)
    go

create table RefreshTokens
(
    Id         int identity
        constraint PK_RefreshTokens
            primary key,
    Token      nvarchar(max)    not null,
    ExpiryDate datetime2        not null,
    IsRevoked  bit              not null,
    CreatedAt  datetime2        not null,
    UserId     uniqueidentifier not null
        constraint FK_RefreshTokens_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade
)
    go

create index IX_RefreshTokens_UserId
    on RefreshTokens (UserId)
    go

create table Roles
(
    roleid    int identity
        constraint PK_Roles
            primary key,
    rolename  nvarchar(100)                 not null,
    cancreate bit default CONVERT([bit], 0) not null,
    canread   bit default CONVERT([bit], 0) not null,
    canedit   bit default CONVERT([bit], 0) not null,
    candelete bit default CONVERT([bit], 0) not null
)
    go

create unique index IX_Roles_rolename
    on Roles (rolename)
    go

create table ShowTypeLookup
(
    typeid   int identity
        constraint PK_ShowTypeLookup
            primary key,
    showtype nvarchar(100) not null
)
    go

create table TicketTypes
(
    tickettypeid int identity
        constraint PK_TicketTypes
            primary key,
    typename     nvarchar(100) not null,
    description  nvarchar(200) not null
)
    go

create table VenueTypeLookup
(
    typeid    int identity
        constraint PK_VenueTypeLookup
            primary key,
    venuetype nvarchar(100) not null
)
    go

create table Venues
(
    venueid       int identity
        constraint PK_Venues
            primary key,
    venuename     nvarchar(150)                 not null,
    locationid    int                           not null
        constraint FK_Venues_Locations_locationid
            references Locations
            on delete cascade,
    typeid        int                           not null
        constraint FK_Venues_VenueTypeLookup_typeid
            references VenueTypeLookup
            on delete cascade,
    maxcapacity   int                           not null,
    seatingplanid int,
    description   nvarchar(max)                 not null,
    contactemail  nvarchar(max)                 not null,
    contactphone  nvarchar(max)                 not null,
    isaccessible  bit default CONVERT([bit], 0) not null,
    venueurl      nvarchar(max)                 not null,
    active        bit default CONVERT([bit], 1) not null,
    createdbyid   int                           not null,
    createdat     datetime2                     not null,
    updatedid     int,
    updatedat     datetime2
)
    go

create table Shows
(
    showid             int identity
        constraint PK_Shows
            primary key,
    showname           nvarchar(150)                 not null,
    venueid            int                           not null
        constraint FK_Shows_Venues_venueid
            references Venues
            on delete cascade,
    showtypeid         int                           not null
        constraint FK_Shows_ShowTypeLookup_showtypeid
            references ShowTypeLookup
            on delete cascade,
    description        nvarchar(max)                 not null,
    agerestrictionid   int                           not null
        constraint FK_Shows_AgeRestrictionLookup_agerestrictionid
            references AgeRestrictionLookup
            on delete cascade,
    warningdescription nvarchar(max)                 not null,
    startdate          datetime2                     not null,
    enddate            datetime2                     not null,
    tickettypeid       int
        constraint FK_Shows_TicketTypes_tickettypeid
            references TicketTypes,
    imagesurl          nvarchar(max)                 not null,
    videosurl          nvarchar(max)                 not null,
    active             bit default CONVERT([bit], 1) not null,
    createdbyid        int                           not null,
    createdat          datetime2                     not null,
    updatedid          int,
    updatedat          datetime2
)
    go

create index IX_Shows_agerestrictionid
    on Shows (agerestrictionid)
    go

create index IX_Shows_showtypeid
    on Shows (showtypeid)
    go

create index IX_Shows_tickettypeid
    on Shows (tickettypeid)
    go

create index IX_Shows_venueid
    on Shows (venueid)
    go

create index IX_Venues_locationid
    on Venues (locationid)
    go

create index IX_Venues_typeid
    on Venues (typeid)
    go

create table __EFMigrationsHistory
(
    MigrationId    nvarchar(150) not null
        constraint PK___EFMigrationsHistory
            primary key,
    ProductVersion nvarchar(32)  not null
)
    go


-- DATA SEEDING
-- Insert roles (already has WHERE NOT EXISTS checks)
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
SELECT NEWID(), 'Admin', 'ADMIN', NEWID()
    WHERE NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Admin');

INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
SELECT NEWID(), 'Manager', 'MANAGER', NEWID()
    WHERE NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Manager');

INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
SELECT NEWID(), 'User', 'USER', NEWID()
    WHERE NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'User');

-- Insert admin user with improved checks
DECLARE @adminEmail NVARCHAR(256) = 'admin@fringe.com';
DECLARE @adminId UNIQUEIDENTIFIER;
DECLARE @passwordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEFoTPa9kvFxBTs6FTLfIHMe9od9d1xIYu0eLzC8SgPRS4NKN49XZ4GiSy0xrwHWs4Q==';
DECLARE @adminRoleId UNIQUEIDENTIFIER;

-- First check if admin user already exists
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Email] = @adminEmail)
BEGIN
    -- Create new admin user
    SET @adminId = NEWID();
    
    INSERT INTO [AspNetUsers] (
        [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
        [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
        [FirstName], [LastName], [CreatedAt], [IsActive],
        [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount]
    )
    VALUES (
        @adminId, @adminEmail, UPPER(@adminEmail), @adminEmail, UPPER(@adminEmail),
        1, @passwordHash, NEWID(), NEWID(),
        'Admin', 'User', GETDATE(), 1,
        0, 0, 1, 0
    );
    
    -- Get admin role ID
    SELECT @adminRoleId = [Id] FROM [AspNetRoles] WHERE [Name] = 'Admin';
    
    -- Assign admin role
    IF @adminRoleId IS NOT NULL
    BEGIN
        INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
        VALUES (@adminId, @adminRoleId);
    END
END
ELSE
BEGIN
    -- Get existing admin user ID
    SELECT @adminId = [Id] FROM [AspNetUsers] WHERE [Email] = @adminEmail;
    
    -- Get admin role ID
    SELECT @adminRoleId = [Id] FROM [AspNetRoles] WHERE [Name] = 'Admin';
    
    -- Ensure existing admin has Admin role if not already assigned
    IF NOT EXISTS (
        SELECT 1 FROM [AspNetUserRoles] 
        WHERE [UserId] = @adminId AND [RoleId] = @adminRoleId
    ) AND @adminId IS NOT NULL AND @adminRoleId IS NOT NULL
    BEGIN
        INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
        VALUES (@adminId, @adminRoleId);
    END
END

-- SEEDING FOR MAIN DATA
-- AgeRestrictionLookup
SET IDENTITY_INSERT AgeRestrictionLookup ON;
INSERT INTO AgeRestrictionLookup (agerestrictionid, code, description) VALUES
                                                                           (1, 'G', 'General - Suitable for all ages'),
                                                                           (2, 'PG', 'Parental Guidance - Parental guidance recommended for younger viewers'),
                                                                           (3, '15+', '15+ - Not recommended for audiences under 15'),
                                                                           (4, 'MA15+', 'MA15+ - Not suitable for people under 15, under 15s must be accompanied by an adult'),
                                                                           (5, '18+', '18+ - Restricted to 18 years and over'),
                                                                           (6, 'R18+', 'R18+ - Restricted to 18 years and over with explicit content');
SET IDENTITY_INSERT AgeRestrictionLookup OFF;

-- ShowTypeLookup
SET IDENTITY_INSERT ShowTypeLookup ON;
INSERT INTO ShowTypeLookup (typeid, showtype) VALUES
                                                  (1, 'Comedy'),
                                                  (2, 'Theatre'),
                                                  (3, 'Cabaret'),
                                                  (4, 'Circus'),
                                                  (5, 'Dance'),
                                                  (6, 'Music'),
                                                  (7, 'Visual Art'),
                                                  (8, 'Magic'),
                                                  (9, 'Film'),
                                                  (10, 'Workshop');
SET IDENTITY_INSERT ShowTypeLookup OFF;

-- VenueTypeLookup
SET IDENTITY_INSERT VenueTypeLookup ON;
INSERT INTO VenueTypeLookup (typeid, venuetype) VALUES
                                                    (1, 'Theatre'),
                                                    (2, 'Bar'),
                                                    (3, 'Garden'),
                                                    (4, 'Tent'),
                                                    (5, 'Community Hall'),
                                                    (6, 'Art Gallery'),
                                                    (7, 'Open Air'),
                                                    (8, 'Hotel'),
                                                    (9, 'Restaurant'),
                                                    (10, 'Warehouse');
SET IDENTITY_INSERT VenueTypeLookup OFF;

-- TicketTypes
SET IDENTITY_INSERT TicketTypes ON;
INSERT INTO TicketTypes (tickettypeid, typename, description) VALUES
                                                                  (1, 'Standard', 'Regular admission ticket'),
                                                                  (2, 'Concession', 'Discounted tickets for students, pensioners, and health care card holders'),
                                                                  (3, 'Family', 'Bundle admission for 2 adults and 2 children'),
                                                                  (4, 'Group', 'Discounted tickets for groups of 6+'),
                                                                  (5, 'FringeMEMBER', 'Special pricing for Fringe members'),
                                                                  (6, 'Preview', 'Discounted tickets for preview shows'),
                                                                  (7, 'BankSA', 'Special pricing for BankSA customers'),
                                                                  (8, 'Honey Pot', 'Industry tickets for Honey Pot registered delegates');
SET IDENTITY_INSERT TicketTypes OFF;

-- STEP 2: Insert data into Locations
SET IDENTITY_INSERT Locations ON;
INSERT INTO Locations (locationid, locationname, address, suburb, postalcode, state, country, latitude, longitude, parkingavailable, active, createdbyid, createdat, updatedid, updatedat) VALUES
                                                                                                                                                                                               (1, 'Garden of Unearthly Delights', 'Rundle Park/Kadlitpina', 'Adelaide', '5000', 'SA', 'Australia', -34.9199, 138.6148, 0, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (2, 'Gluttony', 'Rymill Park/Murlawirrapurka', 'Adelaide', '5000', 'SA', 'Australia', -34.9211, 138.6133, 0, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (3, 'Adelaide Festival Centre', 'King William Road', 'Adelaide', '5000', 'SA', 'Australia', -34.9206, 138.5995, 1, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (4, 'Holden Street Theatres', '34 Holden Street', 'Hindmarsh', '5007', 'SA', 'Australia', -34.9085, 138.5737, 1, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (5, 'RCC', 'University of Adelaide', 'Adelaide', '5000', 'SA', 'Australia', -34.9192, 138.6042, 0, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (6, 'Nexus Arts', 'Lion Arts Centre, North Terrace', 'Adelaide', '5000', 'SA', 'Australia', -34.9212, 138.5939, 0, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (7, 'Rhino Room', '1/131 Pirie Street', 'Adelaide', '5000', 'SA', 'Australia', -34.9269, 138.6079, 0, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (8, 'The GC at The German Club', '223 Flinders Street', 'Adelaide', '5000', 'SA', 'Australia', -34.9311, 138.6078, 1, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (9, 'Tandanya National Aboriginal Cultural Institute', '253 Grenfell Street', 'Adelaide', '5000', 'SA', 'Australia', -34.9243, 138.6134, 1, 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                               (10, 'The Mill', '154 Angas Street', 'Adelaide', '5000', 'SA', 'Australia', -34.9303, 138.6037, 0, 1, 1, GETDATE(), NULL, NULL);
SET IDENTITY_INSERT Locations OFF;

-- STEP 3: Insert data into Venues
SET IDENTITY_INSERT Venues ON;
INSERT INTO Venues (venueid, venuename, locationid, typeid, maxcapacity, seatingplanid, description, contactemail, contactphone, isaccessible, venueurl, active, createdbyid, createdat, updatedid, updatedat) VALUES
                                                                                                                                                                                                                   (1, 'The Vagabond', 1, 4, 350, NULL, 'A beautiful spiegeltent with stained glass and mahogany interior', 'vagabond@gardenofunearthlydelights.com.au', '0412345678', 1, 'https://www.gardenofunearthlydelights.com.au/venues/the-vagabond', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (2, 'Fortuna Spiegeltent', 1, 4, 300, NULL, 'An intimate spiegeltent perfect for cabaret and circus performances', 'fortuna@gardenofunearthlydelights.com.au', '0423456789', 1, 'https://www.gardenofunearthlydelights.com.au/venues/fortuna-spiegeltent', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (3, 'The Peacock', 2, 4, 400, NULL, 'Gluttony''s largest venue, a stunning spiegeltent for major performances', 'peacock@gluttony.net.au', '0434567890', 1, 'https://www.gluttony.net.au/venues/the-peacock', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (4, 'The Octagon', 2, 1, 200, NULL, 'An eight-sided tent with excellent acoustics', 'octagon@gluttony.net.au', '0445678901', 1, 'https://www.gluttony.net.au/venues/the-octagon', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (5, 'Space Theatre', 3, 1, 350, NULL, 'A flexible theatre space with tiered seating', 'spacetheatre@adelaidefestivalcentre.com.au', '0456789012', 1, 'https://www.adelaidefestivalcentre.com.au/venues/space-theatre', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (6, 'The Studio', 4, 1, 80, NULL, 'An intimate black box theatre for close-up performances', 'studio@holdenstreettheatres.com', '0467890123', 1, 'https://www.holdenstreettheatres.com/venues/the-studio', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (7, 'Elder Hall', 5, 1, 500, NULL, 'A grand hall with impressive acoustics and heritage architecture', 'elderhall@rcc.org.au', '0478901234', 1, 'https://www.rcc.org.au/venues/elder-hall', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (8, 'Nexus Live', 6, 6, 150, NULL, 'Contemporary performance space showcasing experimental works', 'live@nexusarts.org.au', '0489012345', 1, 'https://www.nexusarts.org.au/venues/nexus-live', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (9, 'Upstairs at Rhino', 7, 2, 100, NULL, 'Adelaide''s legendary comedy venue', 'upstairs@rhinoroom.com.au', '0491234567', 0, 'https://www.rhinoroom.com.au/venues/upstairs', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (10, 'Bier Hall', 8, 2, 250, NULL, 'Traditional German beer hall transformed for Fringe performances', 'bierhall@thegc.com.au', '0491234569', 1, 'https://www.thegc.com.au/venues/bier-hall', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (11, 'Main Gallery', 9, 6, 200, NULL, 'Showcase space for Indigenous arts and performances', 'gallery@tandanya.com.au', '0491234570', 1, 'https://www.tandanya.com.au/venues/main-gallery', 1, 1, GETDATE(), NULL, NULL),
                                                                                                                                                                                                                   (12, 'The Breakout', 10, 10, 120, NULL, 'Industrial warehouse space transformed for experimental performances', 'breakout@themilladelaide.com', '0491234571', 0, 'https://www.themilladelaide.com/venues/the-breakout', 1, 1, GETDATE(), NULL, NULL);
SET IDENTITY_INSERT Venues OFF;

-- STEP 4: Insert data into Shows
SET IDENTITY_INSERT Shows ON;
INSERT INTO Shows (showid, showname, venueid, showtypeid, description, agerestrictionid, warningdescription, startdate, enddate, tickettypeid, imagesurl, videosurl, active, createdbyid, createdat, updatedid, updatedat) VALUES
                                                                                                                                                                                                                               (1, 'Cirque Nocturne', 1, 4, 'A breathtaking circus show that blends aerial acrobatics, contortion, and live music in a gothic-inspired production that will leave you spellbound.', 2, 'Contains strobe lighting, theatrical haze, and loud noises', '2025-02-14 19:30:00', '2025-03-16 22:00:00', 1, '/images/shows/cirque-nocturne.jpg', '/videos/shows/cirque-nocturne-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (2, 'Comedy Allstars Showcase', 9, 1, 'The best international and Australian comedians come together for a night of non-stop laughter featuring five comedy stars in one unmissable show.', 5, 'Contains strong language, adult themes, and sexual references', '2025-02-15 21:00:00', '2025-03-14 23:30:00', 1, '/images/shows/comedy-allstars.jpg', '/videos/shows/comedy-allstars-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (3, 'Midnight Cabaret', 2, 3, 'A decadent late-night cabaret spectacle featuring burlesque, live jazz, and outrageous performances that push boundaries and celebrate the risqué.', 6, 'Contains nudity, sexual content, strong language, and adult themes', '2025-02-21 23:00:00', '2025-03-15 01:30:00', 5, '/images/shows/midnight-cabaret.jpg', '/videos/shows/midnight-cabaret-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (4, 'Echoes of the Dreamtime', 11, 2, 'An immersive theatrical experience that brings ancient Indigenous Australian stories to life through innovative storytelling, projection mapping, and traditional dance.', 2, 'Contains supernatural themes and loud sounds', '2025-02-18 18:00:00', '2025-03-09 20:00:00', 2, '/images/shows/echoes-dreamtime.jpg', '/videos/shows/echoes-dreamtime-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (5, 'The Time Benders', 5, 2, 'A mind-bending sci-fi play that follows a group of scientists who accidentally discover time travel and must navigate the moral implications of changing the past.', 3, 'Contains mild language, suspenseful scenes, and philosophical themes', '2025-02-20 19:00:00', '2025-03-12 21:30:00', 1, '/images/shows/time-benders.jpg', '/videos/shows/time-benders-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (6, 'Underground Sound Collective', 7, 6, 'A fusion of electronic, jazz, and world music performed by an ensemble of innovative musicians pushing the boundaries of contemporary sound.', 4, 'Contains strobe lighting, theatrical haze, and loud music', '2025-02-25 20:00:00', '2025-03-15 22:30:00', 4, '/images/shows/underground-sound.jpg', '/videos/shows/underground-sound-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (7, 'Illusions of Reality', 3, 8, 'Award-winning illusionist Maya Mystique presents a jaw-dropping show that combines classic magic with modern technology to create impossible moments before your eyes.', 1, 'Contains theatrical haze and sudden surprising moments', '2025-02-14 17:00:00', '2025-03-16 19:00:00', 3, '/images/shows/illusions-reality.jpg', '/videos/shows/illusions-reality-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (8, 'Physical Jerks', 12, 5, 'A high-energy contemporary dance performance exploring themes of connection and isolation in the digital age through powerful choreography and innovative staging.', 3, 'Contains brief strobe lighting and emotionally intense scenes', '2025-02-27 19:30:00', '2025-03-14 21:00:00', 1, '/images/shows/physical-jerks.jpg', '/videos/shows/physical-jerks-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (9, 'The Tiny Art Show', 8, 7, 'A delightful exhibition of miniature artworks created by over 50 artists from around the world, each telling a big story in a small space.', 1, 'No warnings applicable', '2025-02-15 10:00:00', '2025-03-16 18:00:00', 2, '/images/shows/tiny-art-show.jpg', '/videos/shows/tiny-art-show-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (10, 'Comedy Hypnosis Spectacular', 10, 1, 'Master hypnotist Dr. Sleepwell turns audience volunteers into the stars of the show in this hilarious and astonishing demonstration of the power of suggestion.', 4, 'Contains adult humor and audience participation', '2025-02-19 20:00:00', '2025-03-08 22:00:00', 1, '/images/shows/comedy-hypnosis.jpg', '/videos/shows/comedy-hypnosis-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (11, 'Shakespeare Remixed', 6, 2, 'Four of Shakespeare''s most iconic scenes reimagined and set in contemporary Australia, breathing new life into the Bard''s timeless works.', 3, 'Contains mild language and themes of conflict', '2025-02-22 18:30:00', '2025-03-10 20:30:00', 6, '/images/shows/shakespeare-remixed.jpg', '/videos/shows/shakespeare-remixed-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (12, 'Silent Disco Walking Tour', 4, 6, 'Experience Adelaide like never before as you dance through the streets with wireless headphones, guided by charismatic performers who reveal hidden stories of the city.', 2, 'Involves walking approximately 2km through city streets', '2025-02-15 17:00:00', '2025-03-15 19:00:00', 4, '/images/shows/silent-disco-tour.jpg', '/videos/shows/silent-disco-tour-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (13, 'Future Filmmakers Showcase', 3, 9, 'A curated selection of short films from emerging Australian directors, exploring themes of identity, technology, and climate change.', 4, 'Contains some strong language, brief nudity, and themes of environmental crisis', '2025-03-01 15:00:00', '2025-03-15 17:30:00', 2, '/images/shows/future-filmmakers.jpg', '/videos/shows/future-filmmakers-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (14, 'Puppetry Masterclass', 8, 10, 'Learn the art of puppet manipulation and storytelling from internationally acclaimed puppet master Giovanni Russo in this hands-on workshop.', 1, 'No warnings applicable', '2025-03-08 10:00:00', '2025-03-08 13:00:00', 8, '/images/shows/puppetry-masterclass.jpg', '/videos/shows/puppetry-masterclass-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (15, 'Late Night Comedy Roast', 9, 1, 'Adelaide''s sharpest comedians take aim at each other and local celebrities in this no-holds-barred comedy battle of wits.', 5, 'Contains very strong language, crude humor, and adult themes', '2025-02-28 22:30:00', '2025-03-15 00:30:00', 5, '/images/shows/comedy-roast.jpg', '/videos/shows/comedy-roast-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (16, 'Quantum Circus', 1, 4, 'Physical laws seem to bend as acrobats defy gravity and perception in this spectacular circus show inspired by quantum physics.', 1, 'Contains strobe lighting and theatrical haze', '2025-03-05 19:00:00', '2025-03-16 21:00:00', 3, '/images/shows/quantum-circus.jpg', '/videos/shows/quantum-circus-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (17, 'Jazz Under the Stars', 2, 6, 'An evening of smooth jazz performed by the Adelaide Jazz Collective under the summer night sky in a beautiful garden setting.', 2, 'No warnings applicable', '2025-02-14 20:00:00', '2025-03-02 22:30:00', 7, '/images/shows/jazz-stars.jpg', '/videos/shows/jazz-stars-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (18, 'Digital Dreamscapes', 7, 7, 'An interactive digital art installation that responds to movement and sound, creating a unique immersive experience for each visitor.', 1, 'Contains flashing images and immersive environments', '2025-02-20 11:00:00', '2025-03-16 20:00:00', 1, '/images/shows/digital-dreamscapes.jpg', '/videos/shows/digital-dreamscapes-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (19, 'Spoken Word Revolution', 11, 2, 'Powerful poetry and personal stories from diverse voices exploring themes of identity, belonging, and social change in contemporary Australia.', 4, 'Contains strong language and discussions of racism, identity, and trauma', '2025-03-07 19:00:00', '2025-03-16 21:00:00', 2, '/images/shows/spoken-word.jpg', '/videos/shows/spoken-word-promo.mp4', 1, 1, GETDATE(), NULL, NULL),

                                                                                                                                                                                                                               (20, 'Fringe of Physics', 5, 10, 'Scientific demonstrations that border on magic in this educational and entertaining show that makes complex physics accessible and fun for all ages.', 1, 'Contains loud noises and scientific demonstrations', '2025-02-22 14:00:00', '2025-03-09 16:00:00', 3, '/images/shows/fringe-physics.jpg', '/videos/shows/fringe-physics-promo.mp4', 1, 1, GETDATE(), NULL, NULL);
SET IDENTITY_INSERT Shows OFF;