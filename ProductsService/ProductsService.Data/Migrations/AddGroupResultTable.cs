using FluentMigrator;

namespace ProductsService.Data.Migrations
{
    [Migration(202409301)]
    public class AddGroupResultTable : Migration
    {
        public override void Up()
        {
            Create.Table("GroupResults")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("TotalPrice").AsDecimal().NotNullable();

            Alter.Table("Products")
                .AddColumn("GroupResultId").AsInt32().Nullable()
                .ForeignKey("FK_Products_GroupResults", "GroupResults", "Id");
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_Products_GroupResults").OnTable("Products");
            Delete.Column("GroupResultId").FromTable("Products");
            Delete.Table("GroupResults");
        }
    }
}
