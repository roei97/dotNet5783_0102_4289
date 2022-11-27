﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlApi;
using Dal;

namespace BlImplementation
{
    internal class Cart : BlApi.ICart
    {
        private DalApi.IDal Dal = new DalList();
        /// <summary>
        /// add a product to the cart
        /// </summary>
        /// <param name="cart">the shopping cart</param>
        /// <param name="productID">the product's ID</param>
        /// <returns>the updated cart</returns>
        /// <exception cref="BO.Exceptions.InsufficientStockException"></exception>
        /// <exception cref="BO.Exceptions.ObjectNotFoundException"></exception>
        public BO.Cart AddProduct(BO.Cart cart, int productID)
        {
            try
            {
                var product = Dal._product.Get(productID);
                if (product.InStock == 0) throw new BO.Exceptions.InsufficientStockException();
                BO.OrderItem? item;
                try
                {
                    item = cart.Items.First(p => p.ProductId == productID);
                    item.Amount++;//TODO check if this updates the item in the list in the cart
                    item.TotalPrice += product.Price;
                }
                catch
                {
                    item = new BO.OrderItem()
                    {
                        ID = 0,
                        ProductId = productID,
                        Name = product.Name,
                        Price = product.Price,
                        Amount = 1,
                        TotalPrice = product.Price
                    };
                    cart.Items.Add(item); //TODO לעשות את בקשת פרטי מוצר ולקחת משם                     
                }
                cart.TotalPrice += product.Price;
                return cart;
            }
            catch (DO.ObjectNotFoundException)
            {
                throw new BO.Exceptions.ObjectNotFoundException(new DO.ObjectNotFoundException());
            }
        }
        /// <summary>
        /// updates the amount of a product in the cart
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="productID"></param>
        /// <param name="amount">the new amount</param>
        /// <returns>the updated cart</returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="BO.Exceptions.InsufficientStockException"></exception>
        /// <exception cref="BO.Exceptions.ObjectNotFoundException"></exception>
        public BO.Cart UpdateAmount(BO.Cart cart, int productID, int amount)
        {
            if (amount < 0) throw new InvalidDataException();
            try
            {
                var item = cart.Items.First(p => p.ProductId == productID);
                int difference = amount - item.Amount;
                if (Dal._product.Get(productID).InStock < difference) throw new BO.Exceptions.InsufficientStockException();
                if (amount > 0)
                {
                    item.Amount = amount;//TODO check if this updates the item in the list in the cart
                    item.TotalPrice = amount * item.Price;
                }
                else
                {
                    //difference*=item.Price;
                    cart.Items.Remove(item);
                }
                cart.TotalPrice += difference * item.Price;
                return cart;
            }
            catch (InvalidOperationException)
            {
                throw new BO.Exceptions.ObjectNotFoundException();
            }
            catch (DO.ObjectNotFoundException)
            {
                throw new BO.Exceptions.ObjectNotFoundException(new DO.ObjectNotFoundException());
            }
        }
        /// <summary>
        /// checkout and make and order
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="customerName"></param>
        /// <param name="Email"></param>
        /// <param name="address"></param>
        /// <returns>the order made from cart</returns>
        /// <exception cref="BO.Exceptions.InsufficientStockException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="BO.Exceptions.ObjectNotFoundException"></exception>
        public BO.Order Checkout(BO.Cart cart, string customerName, string Email, string address)
        {
            try
            {
                foreach (var item in cart.Items)
                {
                    if (Dal._product.Get(item.ProductId).InStock < item.Amount) throw new BO.Exceptions.InsufficientStockException();//check that all the products exist and that there's enough in stock
                    if (item.Amount <= 0 || customerName == "" || Email == "" || address == "") throw new InvalidDataException();
                    try { new System.Net.Mail.MailAddress(Email); } catch (FormatException) { throw new InvalidDataException(); }//the definition of a valid Email address is disputed (google it),and we settled for .NET's defintion
                }
            }
            catch (DO.ObjectNotFoundException)
            {
                throw new BO.Exceptions.ObjectNotFoundException(new DO.ObjectNotFoundException());
            }
            DO.Order order = new DO.Order()
            {
                ID = 0,
                CustomerName = customerName,
                CustomerEmail = Email,
                CustomerAddress = address,
                OrderDate = DateTime.Now,
                ShipDate = DateTime.MinValue,
                DeliveryDate = DateTime.MinValue,
            };
            int orderID = Dal._order.Add(order);
            foreach (var item in cart.Items)
            {
                DO.OrderItem orderItem = new DO.OrderItem()
                {
                    ID = 0,
                    ProductID = item.ProductId,
                    OrderID = orderID,
                    Price = item.Price,
                    Amount = item.Amount
                };
                Dal._orderItem.Add(orderItem);
                int inStock = Dal._product.Get(item.ProductId).InStock;
                if (inStock < item.Amount) throw new BO.Exceptions.InsufficientStockException();
                inStock -= item.Amount;
            }
            BlImplementation.Order orderb = new BlImplementation.Order();
            return orderb.GetOrderDetails(orderID);
        }
    }
}
