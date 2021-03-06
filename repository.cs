using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using AutoMapper;
using Ninject;
using System.Reflection;
using MVCDAMappWeb.Models.ContentTypes;

namespace MVCDAMappWeb.Infastructure.Repositories
{
    public class SharePointModelRepository<T> : SharePointBaseRepository<T> where T : IListItem 
    {
        protected readonly IRepository<ListItem> SharePointListItemRepository;
        protected string[] Columns;
        protected string ListName;

        public SharePointModelRepository(string listName, ClientContext clientContext, params string[] columns) :
            base(clientContext)
        {
            this.SharePointListItemRepository = new ListItemRepository(listName, clientContext, columns);
            this.Columns = columns;
            this.ListName = listName;
        }

        public override IEnumerable<T> SelectAll()
        {
            var items = this.SharePointListItemRepository.SelectAll();
            var model = this.mapper.Map<List<T>>(items);

            return model;
        }

        public override T SelectByID(object id)
        {
            var item = this.SharePointListItemRepository.SelectByID(id);
            return this.mapper.Map<T>(item);
        }

        private ListItem CreateListItemInstance()
        {
            List list = this.clientContext.Web.Lists.GetByTitle(this.ListName);
            ListItemCreationInformation ci = new ListItemCreationInformation();
            ListItem item = list.AddItem(ci);

            return item;
        }

        public override void Insert(T obj)
        {
            ListItem instance = this.CreateListItemInstance();
            instance = mapper.Map<ListItem>(obj, opts => opts.Items["instance"] = instance);

            this.SharePointListItemRepository.Insert(instance);
            this.Save();
        }

        public override void Update(T obj)
        {
            var item  = this.SharePointListItemRepository.SelectByID(obj.ID);
            
            item = mapper.Map<ListItem>(obj, opts => opts.Items["instance"] = item);
            item.Update();
            this.Save();
        }

        public override void Delete(object id)
        {
            this.SharePointListItemRepository.Delete(id);
        }
    }
}