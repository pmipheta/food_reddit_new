using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodReddit3_day.Migrations
{
    /// <inheritdoc />
    public partial class model_Login_creatpostandregisteralready : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Communities",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Spicy and flavorful dishes from Thailand", "Thai Food" },
                    { 2, "Sushi, ramen, and delicate flavors", "Japanese Food" },
                    { 3, "Kimchi, BBQ, and bold spicy dishes", "Korean Food" },
                    { 4, "Traditional and modern Chinese cuisine", "Chinese Food" },
                    { 5, "Pasta, pizza, and classic Italian flavors", "Italian Food" },
                    { 6, "Burgers, fries, and comfort food", "American Food" },
                    { 7, "Delicious and affordable street eats", "Street Food" },
                    { 8, "Clean eating and nutritious meals", "Healthy Food" },
                    { 9, "Plant-based and cruelty-free dishes", "Vegan Food" },
                    { 10, "Fresh fish, shrimp, and ocean delights", "Seafood" },
                    { 11, "Smoky grilled meats and BBQ dishes", "BBQ & Grilled" },
                    { 12, "Sweet treats, cakes, and pastries", "Desserts" },
                    { 13, "Coffee culture and cozy cafes", "Cafe & Coffee" },
                    { 14, "Quick and convenient meals", "Fast Food" },
                    { 15, "Simple and homemade recipes", "Home Cooking" },
                    { 16, "Luxury and high-end dining experiences", "Fine Dining" },
                    { 17, "Creative mix of different cuisines", "Fusion Food" },
                    { 18, "Hot and fiery dishes for spice lovers", "Spicy Food" },
                    { 19, "Light bites and starters", "Snacks & Appetizers" },
                    { 20, "Juices, smoothies, and drinks", "Drinks & Beverages" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Communities",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}
