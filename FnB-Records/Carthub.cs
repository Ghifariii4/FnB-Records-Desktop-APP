using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Drawing;

namespace FnB_Records
{
    internal class Carthub
    {
    }

    public static class CartHub
    {
        // Event yang akan didengar oleh Mode_POS
        // Parameter: Nama Menu, Harga, Gambar
        // Tambahkan 'int id' di parameter pertama
        // Tambahkan 'int stock' di parameter ke-4
        public static event Action<int, string, double, int, Image> OnItemAdded;

        public static void AddToCart(int id, string name, double price, int stock, Image img)
        {
            OnItemAdded?.Invoke(id, name, price, stock, img);
        }
    }
}
