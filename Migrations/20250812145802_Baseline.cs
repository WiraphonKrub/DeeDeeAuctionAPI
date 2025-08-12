using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeeDeeAuction.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NO-OP baseline: don't create or alter anything
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NO-OP baseline
        }

    }
}
