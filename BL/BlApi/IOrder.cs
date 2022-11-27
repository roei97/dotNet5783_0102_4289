﻿using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlApi
{
    /// <summary>
    /// handles the orders
    /// </summary>
    public interface IOrder
    {
        public IEnumerable<OrderForList> Get();
        public Order GetOrderDetails(int ID);
        public Order UpdateDelivery(int ID);
        public Order UpdateShipping(int ID);
        public OrderTracking Track(int ID);
        public OrderItem Update(int orderID, int productID, int newAmount);
    }
}
