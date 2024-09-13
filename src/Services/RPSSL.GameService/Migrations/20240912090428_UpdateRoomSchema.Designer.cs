﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RPSSL.GameService.Data;

#nullable disable

namespace RPSSL.GameService.Migrations
{
    [DbContext(typeof(GameDbContext))]
    [Migration("20240912090428_UpdateRoomSchema")]
    partial class UpdateRoomSchema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("RPSSL.GameService.Models.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Player1Id")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Player1Move")
                        .HasColumnType("TEXT");

                    b.Property<string>("Player2Id")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Player2Move")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WinnerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("RPSSL.GameService.Models.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Player1")
                        .HasColumnType("TEXT");

                    b.Property<string>("Player2")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("RPSSL.GameService.Models.Game", b =>
                {
                    b.HasOne("RPSSL.GameService.Models.Room", "Room")
                        .WithMany("Games")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("RPSSL.GameService.Models.Room", b =>
                {
                    b.Navigation("Games");
                });
#pragma warning restore 612, 618
        }
    }
}
