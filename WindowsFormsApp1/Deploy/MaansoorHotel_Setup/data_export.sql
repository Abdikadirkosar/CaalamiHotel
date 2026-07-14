USE [hotel1];
GO

-- Table: users
INSERT INTO [users] ([id],[username],[password],[role],[status],[date_register]) VALUES (1,'Admin','admin','Admin','Active','2026-07-03 00:00:00');
INSERT INTO [users] ([id],[username],[password],[role],[status],[date_register]) VALUES (2,'Cabdirisaaq','staff123','staff','Active','2026-07-03 00:00:00');
INSERT INTO [users] ([id],[username],[password],[role],[status],[date_register]) VALUES (3,'Hodan','staff123','staff','Active','2026-07-03 00:00:00');
INSERT INTO [users] ([id],[username],[password],[role],[status],[date_register]) VALUES (4,'Mucaawiye','staff123','staff','Active','2026-07-03 00:00:00');
INSERT INTO [users] ([id],[username],[password],[role],[status],[date_register]) VALUES (5,'maxamed','Admin123','Staff','Active','2026-07-03 00:00:00');
-- Table: rooms
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (1,'101','Single','Qolka Xiddigta',45,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (2,'102','Single','Qolka Badda',45,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (3,'103','Double','Qolka Hargeysa',80,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (4,'104','Double','Qolka Berbera',80,NULL,'Maintenance','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (5,'105','Suite','Qolka Waabari',150,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (6,'201','Single','Qolka Borama',50,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (7,'202','Double','Qolka Burco',90,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (8,'203','Deluxe','Qolka Gacanka',120,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (9,'204','Suite','Qolka Aw-Barre',180,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (10,'205','Deluxe','Qolka Zeylac',120,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (11,'301','Single','Qolka Ceerigaavo',55,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (12,'302','Double','Qolka Laascaanood',100,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (13,'303','Suite','Qolka Golaaha',220,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (14,'304','Deluxe','Qolka Durdur',140,NULL,'Maintenance','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (15,'305','Suite','Qolka Xidigtaha',220,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (16,'401','Penthouse','Qolka Madaxtooye',380,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (17,'402','Penthouse','Qolka Golaha Sare',380,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (18,'403','Executive Suite','Qolka VIP Somaliland',280,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (19,'404','Deluxe','Qolka Xornimada',150,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
INSERT INTO [rooms] ([id],[room_id],[type],[room_name],[price],[image_path],[status],[date_register],[date_update],[date_delete]) VALUES (20,'405','Double','Qolka Guulwade',105,NULL,'Active','2026-07-03 00:00:00',NULL,NULL);
-- Table: customers
-- (table customers not found or empty)
-- Table: bookings
-- (table bookings not found or empty)
-- Table: audit_log
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (1,'System','Bilaabid','Nidaamka database-ka si guul leh ayaa loo diyaariyey — Maansoor Hotel Hargaisa.','2026-07-03 04:15:59');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (2,'System','Xog-gelin','20 qol, 20 martid Soomaali ah, iyo 3 shaqaale ayaa loo diiwaan galiyey.','2026-07-03 04:15:59');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (3,'Admin','Habayn','Xogta hoteelka Maansoor Hotel Hargaisa la habeeyey.','2026-07-03 04:15:59');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (4,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 04:16:14');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (5,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 04:20:46');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (6,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 04:37:35');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (7,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 04:43:24');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (8,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 05:00:27');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (9,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 06:00:06');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (10,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 06:13:14');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (11,'Admin','Login','User ''Admin'' logged in as Admin','2026-07-03 06:21:53');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (12,'Admin','Diiwaan-gelin Shaqaale','Wuxuu diiwaan-geliyey ''maxamed'' oo ah Staff','2026-07-03 06:23:48');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (13,'Admin','Logout','User ''Admin'' logged out','2026-07-03 06:23:54');
INSERT INTO [audit_log] ([id],[username],[action],[details],[action_date]) VALUES (14,'maxamed','Login','User ''maxamed'' logged in as Staff','2026-07-03 06:24:20');
