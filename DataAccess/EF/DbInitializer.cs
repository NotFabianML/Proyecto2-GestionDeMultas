using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.EF.Models;

namespace DataAccess.EF
{
    public class DbInitializer
    {
        public static async Task Initialize(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Crear la base de datos si no existe
            context.Database.EnsureCreated();

            //Crear roles si no existen
            string[] roleNames = { "Administrador", "Usuario Final", "Oficial de Tránsito", "Juez de Tránsito" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Crear un usuario de ejemplo y asignar rol
            //var adminUser = new IdentityUser { 
            //    UserName = "sa",
            //    Email = "admin@nextek.com"
            //};
            //string adminPassword = "Password123!";
            //var user = await userManager.FindByEmailAsync(adminUser.Email);
            //if (user == null)
            //{
            //    var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
            //    if (createAdmin.Succeeded)
            //    {
            //        await userManager.AddToRoleAsync(adminUser, "Administrador");

            //        // Crear en la tabla Usuarios con el UserId de AspNetUsers
            //        var adminDatos = new Usuario
            //        {
            //            UserId = adminUser.Id,  // Relación con AspNetUsers
            //            Nombre = "Administrador",
            //            Apellido1 = "Principal",
            //            Email = adminUser.Email,
            //            Cedula = "12345678",
            //            FechaNacimiento = DateOnly.Parse("01-01-1970"),
            //            Estado = true
            //        };
            //        context.Usuarios.Add(adminDatos);
            //        await context.SaveChangesAsync();
            //    }
            //}
        }
    }
}