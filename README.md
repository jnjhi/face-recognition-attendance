# Face Recognition Attendance System

A desktop app that takes attendance by recognising faces. You point a camera at a room, the app finds every face in the picture, works out who each person is, and logs them as present. People it has never seen are added automatically, so you can fill in their details later.

I built it in 2025, in C# on .NET 8. It has two parts that talk over the network: a Windows desktop client and a server that owns the data.

## What it does

- **Accounts.** Sign up and log in, with e-mail verification, a captcha and password reset by e-mail.
- **Recognition.** Take a picture with the camera. The app draws a box and a name on every face it knows.
- **Attendance.** Every recognised person is logged with the time. There is a view per person and a view for everyone, and both can be exported to a file.
- **People.** Each person has a profile with a photo and details you can edit. Unknown faces become new profiles.
- **Gallery.** The pictures you took, with the people found in each one.

## How it works

```
 camera ──► CLIENT (WPF)                               SERVER (console)
            find faces in the picture                  look for the closest stored face
            turn each face into 128 numbers  ──────►   log attendance, add unknown people
            draw names on the picture        ◄──────   send back who it is
                         encrypted TCP connection
```

The repository holds four projects:

| Project | What it is |
|---|---|
| `FaceRecognitionClient` | The desktop app. WPF, written in the MVVM style, with OpenCV for the camera. |
| `FaceRecognitionServer` | The server. It keeps the accounts, the people, their face data and the attendance log. |
| `DataProtocols` | The message types both sides share, sent as JSON. |
| `Encryption` | Small wrappers around AES and RSA. |

**Recognising a face.** The client uses [dlib](http://dlib.net/) through DlibDotNet. It finds each face, lines it up using five landmarks, and runs it through dlib's ResNet model, which turns a face into a list of 128 numbers. Two pictures of the same person give lists that are close to each other. The server compares the new list with every stored one and accepts the closest match if the distance is under 0.6, the threshold dlib recommends.

**The connection.** Client and server speak their own protocol over TCP. When a client connects, it sends an RSA public key. The server creates an AES key for that client, encrypts it with the RSA key (OAEP with SHA-256) and sends it back. From then on every message is AES-encrypted. Clients that stay silent for ten minutes are disconnected.

**Messages.** Each message carries a type (there are 27). On the server, a pipeline hands every message to the handler registered for its type, so adding a feature means adding a message type and a handler without touching the networking code.

**Data.** Accounts, people, photos and attendance live in a SQL Server LocalDB database (tables `Users`, `Faces`, `ProfilePictures`, `Attendances`). Passwords are stored as salted SHA-256 hashes.

## Running it

You need Windows, the .NET 8 SDK, SQL Server LocalDB and a webcam.

1. Build `DataProtocols` and `Encryption` first. The client and the server reference their DLLs.
2. Create a LocalDB database with the four tables above and point the connection strings at it. They are in the files under `FaceRecognitionServer/Services/DataBases/ConnectionToTables`. The paths in the code assume the project sits in `C:\FaceRecognition`.
3. Download the two dlib model files and unpack them into `FaceRecognitionClient/Services/FaceRecognitionService/FaceRecognitionTrainedModels`:
   - [dlib_face_recognition_resnet_model_v1.dat](http://dlib.net/files/dlib_face_recognition_resnet_model_v1.dat.bz2)
   - [shape_predictor_5_face_landmarks.dat](http://dlib.net/files/shape_predictor_5_face_landmarks.dat.bz2)
4. For the verification e-mails, set two environment variables on the server: `ATTENDANCE_SMTP_USER` (a Gmail address) and `ATTENDANCE_SMTP_APP_PASSWORD` (an app password for it).
5. Start the server, then the client.

The database, the stored photos and the face data are not in this repository, because they describe real people.

## What I would change today

I have learned a lot since I wrote this. If I rebuilt it, I would:

- move the paths, the server address and the connection strings into a configuration file;
- hash passwords with a slow algorithm made for the job, such as Argon2, instead of SHA-256;
- use a fresh IV for every message and an authenticated mode such as AES-GCM;
- search the stored faces with an index instead of comparing against every one;
- add automated tests.
