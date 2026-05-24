using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afetto.Net.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_PAIS",
                columns: table => new
                {
                    ID_PAIS = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SIGLA = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PAIS", x => x.ID_PAIS);
                });

            migrationBuilder.CreateTable(
                name: "TB_ESTADO",
                columns: table => new
                {
                    ID_ESTADO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SIGLA = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    ID_PAIS = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ESTADO", x => x.ID_ESTADO);
                    table.ForeignKey(
                        name: "FK_TB_ESTADO_TB_PAIS_ID_PAIS",
                        column: x => x.ID_PAIS,
                        principalTable: "TB_PAIS",
                        principalColumn: "ID_PAIS",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_CIDADE",
                columns: table => new
                {
                    ID_CIDADE = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ID_ESTADO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CIDADE", x => x.ID_CIDADE);
                    table.ForeignKey(
                        name: "FK_TB_CIDADE_TB_ESTADO_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalTable: "TB_ESTADO",
                        principalColumn: "ID_ESTADO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_BAIRRO",
                columns: table => new
                {
                    ID_BAIRRO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    ID_CIDADE = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_BAIRRO", x => x.ID_BAIRRO);
                    table.ForeignKey(
                        name: "FK_TB_BAIRRO_TB_CIDADE_ID_CIDADE",
                        column: x => x.ID_CIDADE,
                        principalTable: "TB_CIDADE",
                        principalColumn: "ID_CIDADE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_LOGRADOURO",
                columns: table => new
                {
                    ID_LOGRADOURO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CEP = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    TIPO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ID_BAIRRO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_LOGRADOURO", x => x.ID_LOGRADOURO);
                    table.ForeignKey(
                        name: "FK_TB_LOGRADOURO_TB_BAIRRO_ID_BAIRRO",
                        column: x => x.ID_BAIRRO,
                        principalTable: "TB_BAIRRO",
                        principalColumn: "ID_BAIRRO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    CPF = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: false),
                    DATA_NASC = table.Column<DateTime>(type: "DATE", nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    SENHA = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    NUMERO = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    COMPLEMENTO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ID_LOGRADOURO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO", x => x.ID_USUARIO);
                    table.ForeignKey(
                        name: "FK_TB_USUARIO_TB_LOGRADOURO_ID_LOGRADOURO",
                        column: x => x.ID_LOGRADOURO,
                        principalTable: "TB_LOGRADOURO",
                        principalColumn: "ID_LOGRADOURO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_PET",
                columns: table => new
                {
                    ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ESPECIE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    RACA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SEXO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    PESO = table.Column<float>(type: "BINARY_FLOAT", nullable: false),
                    DATA_NASC = table.Column<DateTime>(type: "DATE", nullable: false),
                    DESCRICAO = table.Column<string>(type: "CLOB", nullable: true),
                    QR_CODE_TOKEN = table.Column<string>(type: "NVARCHAR2(36)", maxLength: 36, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PET", x => x.ID_PET);
                    table.ForeignKey(
                        name: "FK_TB_PET_TB_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "TB_USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_BAIRRO_ID_CIDADE",
                table: "TB_BAIRRO",
                column: "ID_CIDADE");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CIDADE_ID_ESTADO",
                table: "TB_CIDADE",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ESTADO_ID_PAIS",
                table: "TB_ESTADO",
                column: "ID_PAIS");

            migrationBuilder.CreateIndex(
                name: "IX_TB_LOGRADOURO_ID_BAIRRO",
                table: "TB_LOGRADOURO",
                column: "ID_BAIRRO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_PET_ID_USUARIO",
                table: "TB_PET",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_PET_QR_CODE_TOKEN",
                table: "TB_PET",
                column: "QR_CODE_TOKEN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_CPF",
                table: "TB_USUARIO",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_EMAIL",
                table: "TB_USUARIO",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_ID_LOGRADOURO",
                table: "TB_USUARIO",
                column: "ID_LOGRADOURO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_PET");

            migrationBuilder.DropTable(
                name: "TB_USUARIO");

            migrationBuilder.DropTable(
                name: "TB_LOGRADOURO");

            migrationBuilder.DropTable(
                name: "TB_BAIRRO");

            migrationBuilder.DropTable(
                name: "TB_CIDADE");

            migrationBuilder.DropTable(
                name: "TB_ESTADO");

            migrationBuilder.DropTable(
                name: "TB_PAIS");
        }
    }
}
