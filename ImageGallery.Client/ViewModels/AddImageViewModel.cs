﻿using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ImageGallery.Client.ViewModels
{
    public class AddImageViewModel
    {
        public List<IFormFile> Files { get; set; }

        public AddImageViewModel()
        {
        }
    }
}