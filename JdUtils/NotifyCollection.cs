using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace JdUtils
{
    /// <summary>
    /// Třída pracující podobně jako <see cref="ObservableCollection{T}"/> s podporou Range.
    /// Nelze použít přímo na bidnování do ItemsControl, protože ItemsControl nepodporuje Range operace
    /// </summary>
    /// <typeparam name="T">Typ prvků</typeparam>
    public class NotifyCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string IndexerName = "Items[]";
        private readonly SimpleMonitor m_monitor = new SimpleMonitor();
        private bool m_rangeUpdate;
        private bool m_isUpdating;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public NotifyCollection()
        {
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="collection"></param>
        public NotifyCollection(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            CopyFrom(collection);
        }

        /// <summary>Occurs when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public IList<NotifyCollectionChangedEventArgs> EventStack { get; } = new List<NotifyCollectionChangedEventArgs>();

        /// <summary>
        /// Přesunout prvek
        /// </summary>
        /// <param name="oldIndex">Původní index prvku</param>
        /// <param name="newIndex">Nový index prvku</param>
        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        #region Range

        /// <summary>
        /// Přidá na konec kolekci prvků
        /// </summary>
        /// <param name="collection">Kolekce přidávaných prvků</param>
        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(Count, collection);
        }

        /// <summary>
        /// Vloží na požadovaný index kolekci prvků
        /// </summary>
        /// <param name="index">Index, kam se má kolekce vložit</param>
        /// <param name="collection">Kolekce vkládaný prvků</param>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            m_rangeUpdate = true;
            var insertedItems = InsertRangeInternal(index, collection);
            RaiseCollectionRangeChanged(NotifyCollectionChangedAction.Add, insertedItems, index);
            m_rangeUpdate = false;
        }

        /// <summary>
        /// Odstraní požadovaný počet prvků z kolekce od vybraného indexu
        /// </summary>
        /// <param name="index">Počáteční index, od kterého budou prvky odebírány</param>
        /// <param name="count">Počet odebíraných prvků</param>
        public void RemoveRange(int index, int count)
        {
            m_rangeUpdate = true;
            var removedItems = RemoveRangeInternal(index, count);
            RaiseCollectionRangeChanged(NotifyCollectionChangedAction.Remove, removedItems, index);
            m_rangeUpdate = false;
        }

        /// <summary>
        /// Nahradí požadovaný počet prvků od vybraného indexu za kolekci nových prvků
        /// </summary>
        /// <param name="index">Počáteční index, od kterého budou prvky nahrazeny</param>
        /// <param name="count">Počet nahrazených prvků</param>
        /// <param name="collection">Kolekce nových prvků</param>
        public void ReplaceRange(int index, int count, IEnumerable<T> collection)
        {
            m_rangeUpdate = true;
            var removeItems = RemoveRangeInternal(index, count);
            var insertedItems = InsertRangeInternal(index, collection);
            RaiseCollectionRangeReplaced(removeItems, insertedItems, index);
            m_rangeUpdate = false;
        }

        /// <summary>
        /// Přesune požadovaný počet prvků na požadovanou pozici
        /// </summary>
        /// <param name="oldIndex">Počáteční index, od kterého budou prvky vybrány</param>
        /// <param name="newIndex">Nový index, kam budou prvky umístěny</param>
        /// <param name="count">Počet vybraných prvků</param>
        public void MoveRange(int oldIndex, int newIndex, int count)
        {
            m_rangeUpdate = true;
            var items = RemoveRangeInternal(oldIndex, count);
            InsertRangeInternal(newIndex, (IEnumerable<T>)items);
            RaiseCollectionRangeMoved(items, oldIndex, newIndex);
            m_rangeUpdate = false;
        }

        #endregion

        /// <summary>
        /// Zabrání odeslání CollectionChanged. Používá se pro hromadnější změny.
        /// Všechny požadované eventy se uloží do <see cref="EventStack"/>
        /// </summary>
        public void BeginUpdate()
        {
            m_isUpdating = true;
            m_rangeUpdate = true;
            EventStack.Clear();
        }

        /// <summary>
        /// Ukončí hromadné změny. Předaným handlerem lze zpracovat všechny nastalé eventy. V opačném případě
        /// se provedou všechny vyvolané změny v pořadí v jakém vznikly.
        /// </summary>
        /// <param name="mergeEventStackHandler"></param>
        public void EndUpdate(Action<IList<NotifyCollectionChangedEventArgs>> mergeEventStackHandler)
        {
            m_rangeUpdate = false;
            m_isUpdating = false;
            mergeEventStackHandler?.Invoke(EventStack);
            HandleEventStack();
            EventStack.Clear();
        }

        /// <summary>
        /// Ukončí hromadné změny. Vyvolá event reset a zahodí všechny nastalé eventy
        /// </summary>
        public void EndUpdate()
        {
            m_rangeUpdate = false;
            m_isUpdating = false;
            EventStack.Clear();
            EventStack.Add(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            HandleEventStack();
            EventStack.Clear();
        }

        /// <summary>
        /// Ošetření <see cref="PropertyChanged"/>
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            CheckReentrancy();
            base.ClearItems();
            RaiseCountChanged();
            RaiseCollectionReseted();
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            var item = this[index];
            base.RemoveItem(index);
            RaiseCountChanged();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);
            RaiseCountChanged();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            var oldItem = this[index];
            base.SetItem(index, item);
            RaiseCountChanged(false);
            RaiseCollectionReplaced(oldItem, item, index);
        }

        /// <summary>
        /// Metoda pro posun prvku z jednoho místa na druhé
        /// </summary>
        /// <param name="oldIndex">Původní umístění</param>
        /// <param name="newIndex">Nové umístění</param>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();
            var item = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);
            RaiseCountChanged(false);
            RaiseCollectionMoved(item, oldIndex, newIndex);
        }

        /// <summary>
        /// Disallows reentrant attempts to change this collection.
        /// INFO Zkopírováno z <see cref="ObservableCollection{T}"/>
        /// </summary>
        /// <returns></returns>
        protected IDisposable BlockReentrancy()
        {
            m_monitor.Enter();
            return m_monitor;
        }

        /// <summary>
        /// Checks for reentrant attempts to change this collection.
        /// INFO Zkopírováno z <see cref="ObservableCollection{T}"/>
        /// </summary>
        protected void CheckReentrancy()
        {
            if (m_monitor.Busy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
            }
        }

        /// <summary>
        /// Vyvolá event o změně kolekce, když došlo k resetu
        /// </summary>
        protected void RaiseCollectionReseted()
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Vyvolá event o změně kolekce, pokud nebylo vyvolána hromadná akce
        /// </summary>
        /// <param name="action">Akce, která změnu vyvolala Add/Remove</param>
        /// <param name="item">Ovlivněný prvek</param>
        /// <param name="index">Index ovlivněného prvku</param>
        protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, T item, int index)
        {
            if (!m_rangeUpdate)
            {
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
            }
        }

        /// <summary>
        /// Vyvolá event o změně kolekce, kdy prvek změnil místo
        /// </summary>
        /// <param name="item">Ovlivněný prvek</param>
        /// <param name="oldIndex">Původní umístění prvku</param>
        /// <param name="newIndex">Nové umístění prvku</param>
        protected void RaiseCollectionMoved(T item, int oldIndex, int newIndex)
        {
            if (!m_rangeUpdate)
            {
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
            }
        }

        /// <summary>
        /// Vyvolá event o změně kolekce, kdy byl prvek nahrazen jiným
        /// </summary>
        /// <param name="oldItem">Původní prvek</param>
        /// <param name="newItem">Nový prvek</param>
        /// <param name="index">Index, na kterém došlo ke změně</param>
        protected void RaiseCollectionReplaced(T oldItem, T newItem, int index)
        {
            if (!m_rangeUpdate)
            {
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
            }
        }

        /// <summary>
        /// Vyvolá event o hromadné změně
        /// </summary>
        /// <param name="action">Akce, která změnu vyvolala Add/Remove</param>
        /// <param name="collection">Ovlivněná kolekce</param>
        /// <param name="index">Index, od kterého kolekce začíná</param>
        protected void RaiseCollectionRangeChanged(NotifyCollectionChangedAction action, IList collection, int index)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, collection, index));
        }

        /// <summary>
        /// Vyvolá event o hromadné změně, kdy došlo k nahrazení kolekce
        /// </summary>
        /// <param name="oldItems">Původní prvky</param>
        /// <param name="newItems">Nové prvky</param>
        /// <param name="index">Počáteční index</param>
        protected void RaiseCollectionRangeReplaced(IList oldItems, IList newItems, int index)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);
            RaiseCollectionChanged(args);
        }

        /// <summary>
        /// Vyvolá event o hromadné změně, kdy došlo k přesunutí kolekce
        /// </summary>
        /// <param name="items">Přesouvané prvky</param>
        /// <param name="oldIndex">Původní počáteční index</param>
        /// <param name="newIndex">Nový počáteční index</param>
        protected void RaiseCollectionRangeMoved(IList items, int oldIndex, int newIndex)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex));
        }

        /// <summary>
        /// Ošetří stack eventů
        /// </summary>
        private void HandleEventStack()
        {
            foreach (var eventArgs in EventStack)
            {
                RaiseCollectionChanged(eventArgs);
            }
        }

        /// <summary>
        /// Metoda pro odstranění požadovaného počtu prvků od zadaného indexu.
        /// Vrací seznam odebraných prvků
        /// </summary>
        /// <param name="index">Počáteční index</param>
        /// <param name="count">Počet prvků</param>
        /// <returns></returns>
        private IList RemoveRangeInternal(int index, int count)
        {
            var removedItems = GetRange(index, count);
            var endIndex = Math.Min(index + count, Items.Count);
            for (var i = endIndex - 1; i >= index; i--)
            {
                RemoveItem(i);
            }

            return removedItems.ToList();
        }

        /// <summary>
        /// Metoda pro vložení zadané kolekce na na místo začínající zadaným indexem
        /// Vrací seznam vkládaných prvků
        /// </summary>
        /// <param name="index">Počáteční index</param>
        /// <param name="collection">Vkládané prvky</param>
        /// <returns></returns>
        private IList InsertRangeInternal(int index, IEnumerable<T> collection)
        {
            var insertedItems = collection.ToList();
            foreach (var item in insertedItems)
            {
                InsertItem(index, item);
                index++;
            }

            return insertedItems;
        }

        /// <summary>
        /// Vrátí prvky ze zadaného intervalu
        /// </summary>
        /// <param name="index">Počáteční index</param>
        /// <param name="count">Počet prvků</param>
        /// <returns></returns>
        private IEnumerable<T> GetRange(int index, int count)
        {
            var result = new List<T>();
            if (index < 0 && index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var endIndex = Math.Min(index + count, Items.Count);
            for (var i = index; i < endIndex; i++)
            {
                result.Add(Items[i]);
            }

            return result;
        }

        /// <summary>
        /// Provede kopii prvků
        /// INFO Zkopírovnáno z <see cref="ObservableCollection{T}"/>
        /// </summary>
        /// <param name="collection">Kopírovaná kolekce</param>
        private void CopyFrom(IEnumerable<T> collection)
        {
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Vyvolá event o změně pořadí prvků a počtu prvků
        /// </summary>
        /// <param name="updateCount">Příznak, že došlo i ke změně počtu prvků</param>
        private void RaiseCountChanged(bool updateCount = true)
        {
            if (!m_rangeUpdate)
            {
                if (updateCount)
                {
                    OnPropertyChanged(nameof(Count));
                }

                OnPropertyChanged(IndexerName);
            }
        }

        /// <summary>
        /// Vyvolá event o změně kolekce
        /// </summary>
        /// <param name="e">Parametry změny</param>
        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (m_isUpdating)
            {
                EventStack.Add(e);
            }
            else
            {
                if (CollectionChanged != null)
                {
                    using (BlockReentrancy())
                    {
                        CollectionChanged?.Invoke(this, e);
                    }
                }
            }
        }

        /// <summary>
        /// Pomocná třída pro blokování.
        /// INFO Zkopírováno z <see cref="ObservableCollection{T}"/>
        /// </summary>
        [TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            private int m_busyCount;

            public bool Busy => m_busyCount > 0;

            public void Enter()
            {
                ++m_busyCount;
            }

            public void Dispose()
            {
                --m_busyCount;
            }
        }
    }
}
