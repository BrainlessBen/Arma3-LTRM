namespace Arma_3_LTRM.Models
{
    public class FileOperationAnalysis
    {
        public int FilesToCreate { get; set; }
        public int FilesToModify { get; set; }
        public int FilesToDelete { get; set; }
        public long TotalDownloadSize { get; set; }
        
        public bool HasOperations => FilesToCreate > 0 || FilesToModify > 0 || FilesToDelete > 0;

        public FileOperationAnalysis()
        {
            FilesToCreate = 0;
            FilesToModify = 0;
            FilesToDelete = 0;
            TotalDownloadSize = 0;
        }
    }
}
