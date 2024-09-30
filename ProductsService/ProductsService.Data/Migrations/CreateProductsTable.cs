using FluentMigrator;

namespace ProductsService.Data.Migrations
{
    [Migration(20240930)]
    public class CreateProductsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Products")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("Unit").AsString(50).NotNullable()
                .WithColumn("PricePerUnit").AsDecimal().NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("IsProcessed").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Table("Products");
        }
    }
}
