<script setup lang="ts">
import {onMounted, ref} from 'vue';
import {useRouter} from 'vue-router';

const router = useRouter();

const newItem = ref('');
const items = ref<string[]>([]);
const quantities = ref<string[]>([]);
const editingState = ref<{
  itemIndex: number | null,
  quantityIndex: number | null
}>({itemIndex: null, quantityIndex: null});

const loadFromSession = (key: string) => JSON.parse(sessionStorage.getItem(key) || '[]');
const saveToSession = (key: string, value: any) => sessionStorage.setItem(key, JSON.stringify(value));

const loadItems = () => {
  items.value = loadFromSession('shopping-list');
  quantities.value = loadFromSession('shopping-list-quantities');
};

const saveItems = () => {
  saveToSession('shopping-list', items.value);
  saveToSession('shopping-list-quantities', quantities.value);
};

const addItem = () => {
  if (!newItem.value.trim()) return;
  const [item, quantity] = newItem.value.split(',').map(part => part.trim());
  items.value.push(item);
  quantities.value.push(quantity || '1');
  newItem.value = '';
  saveItems();
};

const removeItem = (index: number) => {
  items.value.splice(index, 1);
  quantities.value.splice(index, 1);
  saveItems();
};

const updateQuantity = (index: number, event: KeyboardEvent) => {
  if (event.key === 'Enter' && editingState.value.quantityIndex === index) {
    const target = event.target as HTMLInputElement;
    quantities.value[index] = target.value.trim() || '1';
    editingState.value.quantityIndex = null;
    saveItems();
  }
};

const updateItem = (index: number, event: KeyboardEvent) => {
  if (event.key === 'Enter' && editingState.value.itemIndex === index) {
    const target = event.target as HTMLInputElement;
    items.value[index] = target.value.trim();
    editingState.value.itemIndex = null;
    saveItems();
  }
};

const enableEditing = (index: number, type: 'item' | 'quantity') => {
  if (type === 'item') {
    editingState.value.itemIndex = index;
  } else if (type === 'quantity') {
    editingState.value.quantityIndex = index;
  }
};

onMounted(loadItems);
</script>

<template>
  <div>
    <h1 class="caveat-brush-regular">Shopping List</h1>
    <input v-model="newItem" @keyup.enter="addItem"
           placeholder="Add a new item (e.g., 'Apples, 2 kg') &#9166"/>
    <ul>
      <li v-for="(item, index) in items" :key="index">
        <input v-if="editingState.itemIndex === index" type="text" v-model="items[index]"
               @keyup.enter="updateItem(index, $event)" class="item-input"/>
        <span v-else @click="enableEditing(index, 'item')" class="item-name">{{ item }}</span>
        <input v-if="editingState.quantityIndex === index" type="text" v-model="quantities[index]"
               @keyup.enter="updateQuantity(index, $event)" class="quantity-input"/>
        <span v-else @click="enableEditing(index, 'quantity')">{{ quantities[index] }}</span>
        <button @click="removeItem(index)" class="remove-button">&times;</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
div {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
}

input {
  width: 90%;
  height: 5vh;
  font-size: 25px;
  text-align: center;
  border: 2px solid var(--color-primary);
  background: transparent;
  color: var(--color-primary);
}

input:focus {
  border-color: var(--color-primary);
  outline: none;
}

input:focus::placeholder {
  color: transparent;
}

button {
  margin-left: 0.5em;
}

ul {
  list-style-type: none;
  padding: 0;
  width: 90%;
}

li {
  margin: 0.5em 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: var(--primary-color);
}

.item-name {
  width: 60%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

li:nth-child(even) {
  background-color: #fff8dc;
}

.item-input, .quantity-input {
  width: 60%;
  height: auto;
  font-size: inherit;
  margin: 0;
}

.remove-button {
  color: var(--primary-color);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.5em;
}
</style>
