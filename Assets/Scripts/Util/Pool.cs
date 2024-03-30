using System;
using System.Collections.Generic;

namespace Assets.Scripts {
    public class Pool<T> {
        private Queue<T> items;
        private Func<T> allocator;

        public int AmountAvailable => items.Count;

        public Pool(int amountOfItems, Func<T> alloc) {
            if(alloc == null) {
                throw new Exception("Pool allocator cannot be null");
            }

            items = new Queue<T>(amountOfItems);
            for(int i = 0; i < amountOfItems; i++) {
                items.Enqueue(alloc());
            }

            allocator = alloc;
        }

        public T Get() {
            if(items.Count > 0) {
                return items.Dequeue();
            } else {
                return allocator();
            }
        }

        public void Return(T t) {
            items.Enqueue(t);
        }
    }
}
