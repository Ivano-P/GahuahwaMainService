﻿using GahuahwaMainService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GahuahwaMainService.Data;

public class AppDbContext:IdentityDbContext {
   
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
}