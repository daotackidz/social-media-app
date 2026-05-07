using Social.Data.Model.Base;
using Social.Data.Seed.Interface;
using static Social.Data.Model.Base.RecordStatus;

namespace Social.Data.Seed.Models.Base
{
    internal class RecordStatusSeed : ISeed<RecordStatus>
    {
        public RecordStatus[] GetSeedData => GetData();

        private RecordStatus[] GetData()
        {
            var results = new List<RecordStatus>()
            {
                new()
                {
                    Id = Status.SystemUsing,
                    StatusName = "Sử dụng bởi hệ thống",
                    DisplayOrder = 0,
                },
                new()
                {
                    Id = Status.Active,
                    StatusName = "Hoạt động",
                    DisplayOrder = 1,
                },
                new()
                {
                    Id = Status.Deactive,
                    StatusName = "Không hoạt động",
                    DisplayOrder = 2,
                },
                new()
                {
                    Id = Status.Deleted,
                    StatusName = "Đã xoá",
                    DisplayOrder = 3,
                },
            };

            return results.ToArray();
        }
    }
}
