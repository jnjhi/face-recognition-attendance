using FaceRecognitionServer.Services.DataBases.ConnectionToTables;
using System.Threading.Tasks.Sources;

namespace FaceRecognitionServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //using var profilePictureDataBase = new GalleryStorageSystem();
            //profilePictureDataBase.ClearGallery();
            //using var faceDataBase = new ConnectionToFaceTable();
            //faceDataBase.DeleteAllFaceRecords();
            var communicationManager = new CommunicationManager();
        }
        
    }



}
