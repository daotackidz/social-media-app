//namespace Social.Common.Infrastructure.Extensions
//{
//    public static class AppDbContextExtension
//    {
//        public static IServiceCollection AddAppDbContext(this IServiceCollection services)
//        {
//            #region AdminDbContext

//            services.AddDbContext<AdminDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());

//            });



//            services.AddDbContext<AdminChungDbContext>(options =>
//            {
//                options.UseNpgsql();
//            });


//            //services.AddScoped(provider =>
//            //{
//            //    var t = GetConnectionString();
//            //    var b = GetSchema();

//            //    var options = new DbContextOptionsBuilder<AdminDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new AdminChungDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<AdminDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new AdminChungDbContext(options.Options, schema);
//            });


//            #endregion

//            #region TaiLieuHuongDanDbContext

//            services.AddDbContext<TaiLieuHuongDanDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            #endregion

//            #region Danh mục

//            services.AddDbContext<DanhMucDbContext>(options =>
//            {
//                options.UseNpgsql();
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<DanhMucDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new DanhMucDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<DanhMucDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new DanhMucDbContext(options.Options, schema);
//            });

//            #endregion

//            #region Danh mục chung

//            services.AddDbContext<DanhMucChungDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<DanhMucChungDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    //return new DanhMucChungDbContext(options, "core_dmchung");
//            //    // Lấy schema từ config, mặc định là "eof_dmchung"
//            //    var schema = HniFunctionCommons.GetAppSettingValueByKey(HniConstantAppSettings.DanhMucChungSchema) ?? "eof_dmchung";
//            //    return new DanhMucChungDbContext(options, schema);

//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var options = new DbContextOptionsBuilder<DanhMucChungDbContext>();

//                var schema = HniFunctionCommons.GetAppSettingValueByKey(HniConstantAppSettings.DanhMucChungSchema) ?? "eof_dmchung";

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new DanhMucChungDbContext(options.Options, schema);
//            });


//            #endregion

//            #region Hồ sơ công việc

//            services.AddDbContext<HoSoCongViecDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<HoSoCongViecDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new HoSoCongViecDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<HoSoCongViecDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new HoSoCongViecDbContext(options.Options, schema);
//            });

//            #endregion

//            #region Hồ sơ lưu trữ

//            services.AddDbContext<HoSoLuuTruDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<HoSoLuuTruDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new HoSoLuuTruDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<HoSoLuuTruDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new HoSoLuuTruDbContext(options.Options, schema);
//            });


//            #endregion

//            #region Lịch công tác

//            services.AddDbContext<LichCongTacDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<LichCongTacDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new LichCongTacDbContext(options, GetSchema());

//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {
                
//                var schema = GetSchema();
                 
//                var options = new DbContextOptionsBuilder<LichCongTacDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString()); 
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new LichCongTacDbContext(options.Options, schema);
//            });



//            #endregion

//            #region Phòng họp

//            services.AddDbContext<PhongHopDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<PhongHopDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new PhongHopDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<PhongHopDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new PhongHopDbContext(options.Options, schema);
//            });

//            #endregion

//            #region Eoffice

//            services.AddDbContext<EofficeDbContext>(options =>
//            {
//                options.UseNpgsql();

//            });
//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<EofficeDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new EofficeDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<EofficeDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new EofficeDbContext(options.Options, schema);
//            });


//            #endregion

//            #region Hrm

//            services.AddDbContext<HrmDbContext>(options =>
//            {
//                options.UseNpgsql();
//                options.EnableSensitiveDataLogging(true);

//            });
//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<HrmDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new HrmDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<HrmDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new HrmDbContext(options.Options, schema);
//            });

//            #endregion

//            #region Tập tin

//            services.AddDbContext<TapTinDbContext>(options =>
//            {
//                options.UseNpgsql();

//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<TapTinDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new TapTinDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<TapTinDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new TapTinDbContext(options.Options, schema);
//            });


//            #endregion

//            #region Cấu hình

//            services.AddDbContext<CauHinhDbContext>(options =>
//            {
//                options.UseNpgsql();

//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<CauHinhDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new CauHinhDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<CauHinhDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new CauHinhDbContext(options.Options, schema);
//            });

//            #endregion

//            #region Văn bản

//            services.AddDbContext<VanBanDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<VanBanDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new VanBanDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<VanBanDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new VanBanDbContext(options.Options, schema);
//            });

//            #endregion

//            #region GiaoviecDbContext
//            services.AddDbContext<GiaoviecDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });



//            //services.AddScoped(provider =>
//            //{
//            //    var t = GetConnectionString();
//            //    var options = new DbContextOptionsBuilder<GiaoviecDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new GiaoviecDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<GiaoviecDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new GiaoviecDbContext(options.Options, schema);
//            });

//            #endregion

//            #region ErpThongBaoDbContext
//            services.AddDbContext<ErpThongBaoDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });



//            //services.AddScoped(provider =>
//            //{
//            //    var t = GetConnectionString();
//            //    var options = new DbContextOptionsBuilder<GiaoviecDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new GiaoviecDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<ErpThongBaoDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new ErpThongBaoDbContext(options.Options, schema);
//            });

//            #endregion

//            #region ErpLichLamViecDbContext
//            services.AddDbContext<ErpLichLamViecDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<ErpLichLamViecDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new ErpLichLamViecDbContext(options.Options, schema);
//            });

//            #endregion

//            #region VanBanNoiBoDbcontext
//            services.AddDbContext<VanBanNoiBoDbcontext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var t = GetConnectionString();
//            //    var options = new DbContextOptionsBuilder<VanBanNoiBoDbcontext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new VanBanNoiBoDbcontext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<VanBanNoiBoDbcontext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new VanBanNoiBoDbcontext(options.Options, schema);
//            });

//            #endregion

//            #region BpmnDbContext
//            services.AddDbContext<BpmnDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var t = GetConnectionString();
//            //    var options = new DbContextOptionsBuilder<BpmnDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new BpmnDbContext(options, "bpmn");
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = "bpmn";

//                var options = new DbContextOptionsBuilder<BpmnDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new BpmnDbContext(options.Options, schema);
//            });




//            #endregion

//            #region FormDocument

//            services.AddDbContext<FormDocumentDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<FormDocumentDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new FormDocumentDbContext(options, GetSchema());
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<FormDocumentDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new FormDocumentDbContext(options.Options, schema);
//            });

//            #endregion


//            #region KySoDbContext
//            services.AddDbContext<KySoDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var schema = HniFunctionCommons.GetAppSettingValueByKey(HniConstantAppSettings.DanhMucChungSchema) ?? "eof_dmchung";
//            //    var options = new DbContextOptionsBuilder<KySoDbContext>()
//            //                    .UseNpgsql(string.Format(ConfigPara.GetDBConnect(), schema))
//            //                    .Options;

//            //    return new KySoDbContext(options, schema);
//            //});

//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = HniFunctionCommons.GetAppSettingValueByKey(HniConstantAppSettings.DanhMucChungSchema) ?? "eof_dmchung";

//                var options = new DbContextOptionsBuilder<KySoDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new KySoDbContext(options.Options, schema);
//            });

//            #endregion

//            #region số đếm redis cache

//            services.AddDbContext<RedisDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            //services.AddScoped(provider =>
//            //{
//            //    var options = new DbContextOptionsBuilder<RedisDbContext>()
//            //                    .UseNpgsql(GetConnectionString())
//            //                    .Options;

//            //    return new RedisDbContext(options, GetSchema());
//            //});
            
//            //change search_path
//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<RedisDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new RedisDbContext(options.Options, schema);
//            });

//            services.AddDbContext<SoDemDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            services.AddScoped(provider =>
//            {
//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<SoDemDbContext>();
                            
//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new SoDemDbContext(options.Options, schema);
//            });

//            #endregion

//            #region TTDH

//            services.AddDbContext<ThongTinDieuHanhDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            services.AddScoped(provider =>
//            {

//                var schema = GetSchema();

//                var options = new DbContextOptionsBuilder<ThongTinDieuHanhDbContext>();

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new ThongTinDieuHanhDbContext(options.Options, schema);
//            });


//            #endregion

//            #region Template Email.

//            services.AddDbContext<EmailTemplateDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });
 

//            //change search_path
//            services.AddScoped(provider =>
//            {
                
//                var options = new DbContextOptionsBuilder<EmailTemplateDbContext>();

//                var schema = HniFunctionCommons.GetAppSettingValueByKey(HniConstantAppSettings.DanhMucChungSchema) ?? "eof_dmchung";

//                //phải bỏ search_path trong connection string.
//                options.UseNpgsql(GetConnectionString());
//                //thêm search_path sau khi mở kết nối.
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new EmailTemplateDbContext(options.Options, schema);
//            });

//            #endregion

//            #region QuanLyXeDbContext

//            services.AddDbContext<QuanLyXeDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            services.AddScoped(provider =>
//            {
//                var schema = GetSchema();
//                var options = new DbContextOptionsBuilder<QuanLyXeDbContext>();

//                options.UseNpgsql(GetConnectionString());
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new QuanLyXeDbContext(options.Options, schema);
//            });

//            #endregion

//            #region QuanLyGiaoBanDbContext

//            services.AddDbContext<QuanLyGiaoBanDbContext>(options =>
//            {
//                options.UseNpgsql(ConfigPara.GetAdminConnect());
//            });

//            services.AddScoped(provider =>
//            {
//                var schema = GetSchema();
//                var options = new DbContextOptionsBuilder<QuanLyGiaoBanDbContext>();

//                options.UseNpgsql(GetConnectionString());
//                options.AddInterceptors(new SchemaInterceptor(schema));

//                return new QuanLyGiaoBanDbContext(options.Options, schema);
//            });

//            #endregion

//            return services;
//        }

//        static string GetSchema() => HniSessionUtilities.Current?.Session?.GetSessionUserInfo()?.Schema ?? "UNKNOWN";
//        static string GetConnectionString()
//        {
//            return string.Format(ConfigPara.GetDBConnect(), GetSchema());
//        }

//    }
//}
