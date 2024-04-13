using Bogus;
using FileBlazor.ServiceModel.Types;
using ServiceStack.Azure.Storage;
using ServiceStack.Data;
using ServiceStack.IO;
using ServiceStack.OrmLite;
using System.Data;

namespace FileBlazor
{
    public static class ConfigureDbFileBlazor
    {
        private static Faker<S3FileItem> fileItemS3Faker = new Faker<S3FileItem>()
            .RuleFor(x => x.AppUserId, faker => faker.Random.Int(1, 4))
            .RuleFor(x => x.FileAccessType, faker => faker.Random.Enum<FileAccessType>());

        private static Faker<AzureFileItem> fileItemAzureFaker = new Faker<AzureFileItem>()
            .RuleFor(x => x.AppUserId, faker => faker.Random.Int(1, 4))
            .RuleFor(x => x.FileAccessType, faker => faker.Random.Enum<FileAccessType>());

        private static Faker<FileSystemFileItem> fileItemFsFaker = new Faker<FileSystemFileItem>()
            .RuleFor(x => x.AppUserId, faker => faker.Random.Int(1, 4))
            .RuleFor(x => x.FileAccessType, faker => faker.Random.Enum<FileAccessType>());

        public static async Task SeedFileBlazor(this IDbConnection db, S3VirtualFiles? s3Vfs,
            AzureBlobVirtualFiles? azureVfs, FileSystemVirtualFiles? fsVfs, string sampleFilesDir)
        {
            var seedData = db.CreateTableIfNotExists<FileSystemFile>();
            db.CreateTableIfNotExists<S3File>();
            db.CreateTableIfNotExists<AzureFile>();
            db.CreateTableIfNotExists<FileSystemFileItem>();
            db.CreateTableIfNotExists<S3FileItem>();
            db.CreateTableIfNotExists<AzureFileItem>();

            if (seedData)
            {
                var seed = 1807832753;
                Randomizer.Seed = new Random(seed);
                if (s3Vfs != null)
                {
                    await SeedSampleFilesForVfs<S3File, S3FileItem>(sampleFilesDir, s3Vfs, db, fileItemS3Faker);
                }
                if (azureVfs != null)
                {
                    await SeedSampleFilesForVfs<AzureFile, AzureFileItem>(sampleFilesDir, azureVfs, db, fileItemAzureFaker);
                }
                if (fsVfs != null)
                {
                    await SeedSampleFilesForVfs<FileSystemFile, FileSystemFileItem>(sampleFilesDir, fsVfs, db, fileItemFsFaker);
                }
            }
        }

        private static async Task SeedSampleFilesForVfs<T, TItem>(string sampleFilesDir, IVirtualFiles vfs,
            IDbConnection db, Faker<TItem> faker)
            where T : class, IFile, new()
            where TItem : class, IFileItem
        {
            var sampleFiles = Directory.GetFiles(sampleFilesDir);
            var fileFaker = new Faker();
            var allRoles = new[] { "Admin", "Manager", "Employee" };
            foreach (var sampleFile in sampleFiles)
            {
                var sampleFileInfo = new FileInfo(sampleFile);
                var fileName = fileFaker.Commerce.ProductName().ToLower().Replace(" ", "_") + sampleFile.GetExtension();
                var fileItem = faker.Generate();
                if (fileItem.FileAccessType == FileAccessType.Team)
                {
                    fileItem.RoleName = fileFaker.PickRandom(allRoles);
                }

                var uploadPathName = vfsUploadPathMapper[typeof(T)];
                var filePath = CreateFilePath(uploadPathName, fileItem.FileAccessType, fileName);
                var relativePath = filePath.Replace("/uploads/", "/");
                var fileExists = vfs.FileExists(relativePath);
                if (!fileExists)
                    await vfs.WriteFileAsync(relativePath, sampleFileInfo.ReadFully());
                var fileItemId = (int)db.Insert(fileItem, selectIdentity: true);
                var file = new T
                {
                    FilePath = filePath,
                    FileName = fileName,
                    ContentLength = sampleFileInfo.Length,
                    ContentType = MimeTypes.GetMimeType(sampleFileInfo.Name),
                    SharedFileId = fileItemId
                };
                db.Insert(file);
            }
        }

        private static Dictionary<Type, string> vfsUploadPathMapper = new()
        {
            { typeof(S3File), "s3" },
            { typeof(AzureFile), "azure" },
            { typeof(FileSystemFile), "fs" }
        };

        private static string CreateFilePath(string uploadLocation, FileAccessType? fileAccessType, string fileName)
        {
            var path = $"/uploads/{uploadLocation}/{fileAccessType}/{fileName}";
            return path;
        }

        private static void UploadFile(IVirtualFiles vfs, IDbConnection db, FileInfo fileInfo, IFileItem fileItem,
            IFile file)
        {
        }
    }
}