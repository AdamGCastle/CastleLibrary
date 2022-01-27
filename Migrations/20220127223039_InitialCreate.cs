using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CastleLibrary.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LibraryUsers",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    NumBooksOut = table.Column<int>(nullable: false),
                    IsGoldMember = table.Column<bool>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: false),
                    MaxBooksAllowed = table.Column<int>(nullable: false),
                    FinesPaid = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryUsers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: false),
                    AuthorID = table.Column<int>(nullable: false),
                    IsAvailable = table.Column<bool>(nullable: false),
                    DueInDate = table.Column<DateTime>(nullable: true),
                    BorrowedByID = table.Column<int>(nullable: true),
                    YearPublished = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Books_Authors_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Authors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Books_LibraryUsers_BorrowedByID",
                        column: x => x.BorrowedByID,
                        principalTable: "LibraryUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanRecords",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookID = table.Column<int>(nullable: false),
                    LibraryUserID = table.Column<int>(nullable: false),
                    DateBorrowed = table.Column<DateTime>(nullable: false),
                    DateReturned = table.Column<DateTime>(nullable: true),
                    DateDue = table.Column<DateTime>(nullable: false),
                    IsReturned = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LoanRecords_Books_BookID",
                        column: x => x.BookID,
                        principalTable: "Books",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanRecords_LibraryUsers_LibraryUserID",
                        column: x => x.LibraryUserID,
                        principalTable: "LibraryUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorID",
                table: "Books",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Books_BorrowedByID",
                table: "Books",
                column: "BorrowedByID");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecords_BookID",
                table: "LoanRecords",
                column: "BookID");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecords_LibraryUserID",
                table: "LoanRecords",
                column: "LibraryUserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanRecords");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "LibraryUsers");
        }
    }
}
