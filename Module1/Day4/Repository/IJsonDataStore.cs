using System;
using System.Threading.Tasks;
using StudentManagement.Domain;

namespace StudentManagement.Repository
{
    public interface IJsonDataStore
    {

        Task SaveAsync(DataSnapshot snapshot);
        Task<DataSnapshot> LoadAsync();

      
        Task<TResult> ReadAsync<TResult>(Func<DataSnapshot, TResult> selector);


        Task<TResult> UpdateAsync<TResult>(Func<DataSnapshot, TResult> mutator);
    }
}