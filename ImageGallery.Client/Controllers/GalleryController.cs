﻿using ImageGallery.Client.Models;
using ImageGallery.Client.Services;
using ImageGallery.Client.ViewModels;
using ImageGallery.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ImageGallery.Client
{
    public class GalleryController : Controller
    {
        private const string ApplicationJsonMediaType = "application/json";
        private readonly IImageGalleryHttpClient imageGalleryHttpClient;

        public GalleryController(IImageGalleryHttpClient imageGalleryHttpClient)
        {
            this.imageGalleryHttpClient = imageGalleryHttpClient;
        }

        public async Task<IActionResult> Index()
        {
            // call the API
            var httpClient = imageGalleryHttpClient.GetClient();

            var response = await httpClient.GetAsync("api/images");

            if (response.IsSuccessStatusCode)
            {
                var imagesAsString = await response.Content.ReadAsStringAsync();
                var images = JsonConvert.DeserializeObject<IList<Image>>(imagesAsString);
                var galleryIndexViewModel = new GalleryIndexViewModel(images);
                return View(galleryIndexViewModel);
            }

            throw new Exception($"A problem happend while calling the API: {response.ReasonPhrase}");
        }

        public async Task<IActionResult> EditImage(Guid id)
        {
            // call the API
            var httpClient = imageGalleryHttpClient.GetClient();
            var response = await httpClient.GetAsync($"api/images/{id}");
            if (response.IsSuccessStatusCode)
            {
                var imageAsString = await response.Content.ReadAsStringAsync();
                var image = JsonConvert.DeserializeObject<Image>(imageAsString);
                var editImageViewModel = new EditImageViewModel
                {
                    Id = image.Id,
                    Title = image.Title,
                };
                return View(editImageViewModel);
            }
            throw new Exception($"A problem happend while calling the API: {response.ReasonPhrase}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditImageViewModel editImageViewModel)
        {
            if (ModelState.IsValid == false)
            {
                return View();
            }

            // create an ImageForUpdate instatnce
            var imageForUpdate = new ImageForUpdate
            {
                Title = editImageViewModel.Title,
            };
            var serializedImageForUpdate = JsonConvert.SerializeObject(imageForUpdate);

            // call the API
            var httpClient = imageGalleryHttpClient.GetClient();
            var requestUri = $"api/images/{editImageViewModel.Id}";
            var content = new StringContent(serializedImageForUpdate, Encoding.Unicode, ApplicationJsonMediaType);
            var response = await httpClient.PutAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            throw new Exception($"A problem happend while calling the API: {response.ReasonPhrase}");
        }

        public async Task<IActionResult> DeleteImage(Guid id)
        {
            // call the API
            var httpClient = imageGalleryHttpClient.GetClient();
            var response = await httpClient.DeleteAsync($"api/images/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            throw new Exception($"A problem happend while calling the API: {response.ReasonPhrase}");
        }

        public IActionResult AddImage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddImageViewModel addImageViewModel)
        {
            if (ModelState.IsValid == false)
            {
                return View();
            }

            // create an ImageForCreation instance
            var imageForCreation = new ImageForCreation
            {
                Title = addImageViewModel.Title,
            };

            // take the first (only) file in the Files list
            var imageFile = addImageViewModel.Files.First();
            if (imageFile.Length > 0)
            {
                using (var fileStrem = imageFile.OpenReadStream())
                using (var ms = new MemoryStream())
                {
                    await fileStrem.CopyToAsync(ms);
                    imageForCreation.Bytes = ms.ToArray();
                }
            }

            // serialize it
            var serializedImageForCreation = JsonConvert.SerializeObject(imageForCreation);

            // call the API
            var httpClient = imageGalleryHttpClient.GetClient();
            var content = new StringContent(serializedImageForCreation, Encoding.Unicode, ApplicationJsonMediaType);
            var response = await httpClient.PostAsync("api/images", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            throw new Exception($"A problem happend while calling the API: {response.ReasonPhrase}");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
