using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sqo;
using Sqo.Transactions;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class SiaqodbWithAutoSize : ISiaqodb
    {
        const int criticalSpaceInBytes = 5 * 1024 * 1024;
        private readonly IDocumentSerializer documentSerializer;
        private readonly string pathToDatabase;
        private Siaqodb db;

        public SiaqodbWithAutoSize(IDocumentSerializer documentSerializer, string pathToDatabase)
        {
            this.documentSerializer = documentSerializer;

            this.ConfigureDatabase();

            this.pathToDatabase = pathToDatabase;
            this.db = new Siaqodb(pathToDatabase);
        }

        private void ConfigureDatabase()
        {
            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            SiaqodbConfigurator.SetDocumentSerializer(this.documentSerializer);
            SiaqodbConfigurator.AddDocument("Document", typeof(QuestionnaireDocumentView));
            SiaqodbConfigurator.AddDocument("Model", typeof(QuestionnaireModelView));
            SiaqodbConfigurator.AddText("JsonEvent", typeof(EventView));
            SiaqodbConfigurator.AddText("Title", typeof(QuestionnaireView));
            SiaqodbConfigurator.AddText("LastInterviewerOrSupervisorComment", typeof(InterviewView));
            SiaqodbConfigurator.AddText("QuestionText", typeof(InterviewAnswerOnPrefilledQuestionView));
            SiaqodbConfigurator.AddText("Answer", typeof(InterviewAnswerOnPrefilledQuestionView));
            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Utc);
            SiaqodbConfigurator.PropertyUseField("Id", "_id", typeof(IPlainStorageEntity));
            SiaqodbConfigurator.EncryptedDatabase = true;
            SiaqodbConfigurator.SetEncryptionPassword("q=5+yaQqS0K!rWaw8FmLuRDWj8XpwI04Yr4MhtULYmD3zX+W+g");
        }

        private async Task IncreaseDatabaseSizeIfNeededAsync()
        {
            if (this.db.DbInfo.FreeSpace < criticalSpaceInBytes)
            {
                var currentMaxSizeOfDb = this.db.DbInfo.MaxSize;

                await this.db.CloseAsync();
                this.db.Dispose();

                this.db = new Siaqodb(this.pathToDatabase, currentMaxSizeOfDb * 2);
            }
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        public ITransaction BeginTransaction()
        {
            return this.db.BeginTransaction();
        }

        public ISqoQuery<T> Cast<T>()
        {
            return this.db.Cast<T>();
        }

        public void Close()
        {
            this.db.Close();
        }

        public int Count<T>()
        {
            return this.db.Count<T>();
        }

        public void Delete(object obj)
        {
            this.db.Delete(obj);
        }

        public void Delete(object obj, ITransaction transaction)
        {
            this.db.Delete(obj, transaction);
        }

        public bool DeleteObjectBy(object obj, params string[] fieldNames)
        {
            return this.db.DeleteObjectBy(obj, fieldNames);
        }

        public bool DeleteObjectBy(object obj, ITransaction transaction, params string[] fieldNames)
        {
            return this.db.DeleteObjectBy(obj, transaction, fieldNames);
        }

        public bool DeleteObjectBy(string fieldName, object obj)
        {
            return this.db.DeleteObjectBy(fieldName, obj);
        }

        public int DeleteObjectBy(Type objectType, Dictionary<string, object> criteria)
        {
            return this.db.DeleteObjectBy(objectType, criteria);
        }

        public int DeleteObjectBy<T>(Dictionary<string, object> criteria)
        {
            return this.db.DeleteObjectBy<T>(criteria);
        }

        public void DropType(Type type)
        {
            this.db.DropType(type);
        }

        public void DropType<T>()
        {
            this.db.DropType<T>();
        }

        public void Flush()
        {
            this.db.Flush();
        }

        public List<MetaType> GetAllTypes()
        {
            return this.db.GetAllTypes();
        }

        public string GetDBPath()
        {
            return this.db.GetDBPath();
        }

        public int GetOID(object obj)
        {
            return this.db.GetOID(obj);
        }

        public IObjectList<T> LoadAll<T>()
        {
            return this.db.LoadAll<T>();
        }

        public IObjectList<T> LoadAllLazy<T>()
        {
            return this.db.LoadAllLazy<T>();
        }

        public List<int> LoadAllOIDs(MetaType type)
        {
            return this.db.LoadAllOIDs(type);
        }

        public T LoadObjectByOID<T>(int oid)
        {
            return this.db.LoadObjectByOID<T>(oid);
        }

        public List<int> LoadOids<T>(Expression expression)
        {
            return this.db.LoadOids<T>(expression);
        }

        public object LoadValue(int oid, string fieldName, MetaType mt)
        {
            return this.db.LoadValue(oid, fieldName, mt);
        }

        public ISqoQuery<T> Query<T>()
        {
            return this.db.Query<T>();
        }

        public void StoreObject(object obj)
        {
            this.IncreaseDatabaseSizeIfNeededAsync().Wait();
            this.db.StoreObject(obj);
        }

        public void StoreObject(object obj, ITransaction transaction)
        {
            this.IncreaseDatabaseSizeIfNeededAsync().Wait();
            this.db.StoreObject(obj, transaction);
        }

        public void StoreObjectPartially(object obj, params string[] properties)
        {
            this.IncreaseDatabaseSizeIfNeededAsync().Wait();
            this.db.StoreObjectPartially(obj, properties);
        }

        public void StoreObjectPartially(object obj, bool onlyReferences, params string[] properties)
        {
            this.IncreaseDatabaseSizeIfNeededAsync().Wait();
            this.db.StoreObjectPartially(obj, onlyReferences, properties);
        }

        public bool UpdateObjectBy(object obj, params string[] fieldNames)
        {
            return this.db.UpdateObjectBy(obj, fieldNames);
        }

        public bool UpdateObjectBy(object obj, ITransaction transaction, params string[] fieldNames)
        {
            return this.db.UpdateObjectBy(obj, transaction, fieldNames);
        }

        public bool UpdateObjectBy(string fieldName, object obj)
        {
            return this.db.UpdateObjectBy(fieldName, obj);
        }

        public Task CloseAsync()
        {
            return this.db.CloseAsync();
        }

        public Task<int> CountAsync<T>()
        {
            return this.db.CountAsync<T>();
        }

        public Task DeleteAsync(object obj)
        {
            return this.db.DeleteAsync(obj);
        }

        public Task DeleteAsync(object obj, ITransaction transaction)
        {
            return this.db.DeleteAsync(obj, transaction);
        }

        public Task<bool> DeleteObjectByAsync(object obj, params string[] fieldNames)
        {
            return this.db.DeleteObjectByAsync(obj, fieldNames);
        }

        public Task<bool> DeleteObjectByAsync(object obj, ITransaction transaction, params string[] fieldNames)
        {
            return this.db.DeleteObjectByAsync(obj, transaction, fieldNames);
        }

        public Task<bool> DeleteObjectByAsync(string fieldName, object obj)
        {
            return this.db.DeleteObjectByAsync(fieldName, obj);
        }

        public Task<int> DeleteObjectByAsync(Type objectType, Dictionary<string, object> criteria)
        {
            return this.db.DeleteObjectByAsync(objectType, criteria);
        }

        public Task<int> DeleteObjectByAsync<T>(Dictionary<string, object> criteria)
        {
            return this.db.DeleteObjectByAsync<T>(criteria);
        }

        public Task DropTypeAsync(Type type)
        {
            return this.db.DropTypeAsync(type);
        }

        public Task DropTypeAsync<T>()
        {
            return this.db.DropTypeAsync<T>();
        }

        public Task FlushAsync()
        {
            return this.db.FlushAsync();
        }

        public Task<List<MetaType>> GetAllTypesAsync()
        {
            return this.db.GetAllTypesAsync();
        }

        public Task<IObjectList<T>> LoadAllAsync<T>()
        {
            return this.db.LoadAllAsync<T>();
        }

        public Task<IObjectList<T>> LoadAllLazyAsync<T>()
        {
            return this.db.LoadAllLazyAsync<T>();
        }

        public Task<List<int>> LoadAllOIDsAsync(MetaType type)
        {
            return this.db.LoadAllOIDsAsync(type);
        }

        public Task<T> LoadObjectByOIDAsync<T>(int oid)
        {
            return this.db.LoadObjectByOIDAsync<T>(oid);
        }

        public Task<List<int>> LoadOidsAsync<T>(Expression expression)
        {
            return this.db.LoadOidsAsync<T>(expression);
        }

        public Task<object> LoadValueAsync(int oid, string fieldName, MetaType mt)
        {
            return this.db.LoadValueAsync(oid, fieldName, mt);
        }

        public async Task StoreObjectAsync(object obj)
        {
            await IncreaseDatabaseSizeIfNeededAsync();
            await this.db.StoreObjectAsync(obj);
        }

        public async Task StoreObjectAsync(object obj, ITransaction transaction)
        {
            await IncreaseDatabaseSizeIfNeededAsync();
            await this.db.StoreObjectAsync(obj, transaction);
        }

        public async Task StoreObjectPartiallyAsync(object obj, params string[] properties)
        {
            await IncreaseDatabaseSizeIfNeededAsync();
            await this.db.StoreObjectPartiallyAsync(obj, properties);
        }

        public async Task StoreObjectPartiallyAsync(object obj, bool onlyReferences, params string[] properties)
        {
            await IncreaseDatabaseSizeIfNeededAsync();
            await this.db.StoreObjectPartiallyAsync(obj, onlyReferences, properties);
        }

        public Task<bool> UpdateObjectByAsync(object obj, params string[] fieldNames)
        {
            return this.db.UpdateObjectByAsync(obj, fieldNames);
        }

        public Task<bool> UpdateObjectByAsync(object obj, ITransaction transaction, params string[] fieldNames)
        {
            return this.db.UpdateObjectByAsync(obj, transaction, fieldNames);
        }

        public Task<bool> UpdateObjectByAsync(string fieldName, object obj)
        {
            return this.db.UpdateObjectByAsync(fieldName, obj);
        }

        public event EventHandler<DeletedEventsArgs> DeletedObject
        {
            add { this.db.DeletedObject += value; }
            remove { this.db.DeletedObject -= value; }
        }
        public event EventHandler<DeletingEventsArgs> DeletingObject
        {
            add { this.db.DeletingObject += value; }
            remove { this.db.DeletingObject -= value; }
        }
        public event EventHandler<LoadedObjectEventArgs> LoadedObject
        {
            add { this.db.LoadedObject += value; }
            remove { this.db.LoadedObject -= value; }
        }
        public event EventHandler<LoadingObjectEventArgs> LoadingObject
        {
            add { this.db.LoadingObject += value; }
            remove { this.db.LoadingObject -= value; }
        }
        public event EventHandler<SavedEventsArgs> SavedObject
        {
            add { this.db.SavedObject += value; }
            remove { this.db.SavedObject -= value; }
        }
        public event EventHandler<SavingEventsArgs> SavingObject
        {
            add { this.db.SavingObject += value; }
            remove { this.db.SavingObject -= value; }
        }
    }
}