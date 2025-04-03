<script setup lang="ts">
import {onMounted, ref} from 'vue'
import {useAuthStore} from '@/stores/auth'
import {useRouter} from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const newItem = ref('')
const items = ref<string[]>([])
const quantities = ref<string[]>([])
const isEditing = ref<number | null>(null)

const loadItems = () => {
  const storedItems = sessionStorage.getItem('shopping-list')
  const storedQuantities = sessionStorage.getItem('shopping-list-quantities')
  if (storedItems) {
    items.value = JSON.parse(storedItems)
  }
  if (storedQuantities) {
    quantities.value = JSON.parse(storedQuantities)
  }
}

const saveItems = () => {
  sessionStorage.setItem('shopping-list', JSON.stringify(items.value))
  sessionStorage.setItem('shopping-list-quantities', JSON.stringify(quantities.value))
}

const addItem = () => {
  if (newItem.value.trim()) {
    const [item, quantity] = newItem.value.split(',').map(part => part.trim())
    items.value.push(item)
    quantities.value.push(quantity || '1')
    newItem.value = ''
    saveItems()
  }
}

const removeItem = (index: number) => {
  items.value.splice(index, 1)
  quantities.value.splice(index, 1)
  saveItems()
}

const updateQuantity = (index: number, event: KeyboardEvent) => {
  if (event.key === 'Enter') {
    const target = event.target as HTMLInputElement
    quantities.value[index] = target.value.trim() || '1'
    isEditing.value = null
    saveItems()
  }
}

const enableEditing = (index: number) => {
  isEditing.value = index
}

const logout = () => {
  authStore.logout()
  localStorage.removeItem('isAuthenticated')
  router.push('/login')
}

onMounted(loadItems)
</script>

<template>
  <div>
    <h1 class="caveat-brush-regular">Shopping List</h1>
    <input v-model="newItem" @keyup.enter="addItem" placeholder="Add a new item &#9166"/>
    <ul>
      <li v-for="(item, index) in items" :key="index">
        {{ item }}
        <input
          v-if="isEditing === index"
          type="text"
          v-model="quantities[index]"
          @keyup.enter="updateQuantity(index, $event)"
          class="quantity-input"
        />
        <span v-else @click="enableEditing(index)">{{ quantities[index] }}</span>
        <button @click="removeItem(index)" class="remove-button">&times;</button>
      </li>
    </ul>
    <button @click="logout" class="logout-button">Logout</button>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Caveat+Brush&display=swap');

div {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
}

.caveat-brush-regular {
  font-family: "Caveat Brush", cursive;
  font-weight: 400;
  font-style: normal;
}

h1 {
  font-size: 3em;
  text-align: center;
  color: var(--color-primary);
  margin: 0 0 1rem;
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
  opacity: 0;
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
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: var(--primary-color);
}

li:nth-child(even) {
  background-color: #fff8dc;
}

.quantity-input {
  width: 35%;
  height: 20px;
  text-align: center;
  margin-left: 1em;
  font-size: 1em;
}

.quantity-input.no-arrows::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.quantity-input.no-arrows {
  -moz-appearance: textfield;
}

span {
  font-size: 1em;
}

.remove-button {
  color: var(--primary-color);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.5em;
}

.logout-button {
  position: absolute;
  top: 10px;
  right: 10px;
  padding: 10px 20px;
  font-size: 16px;
  cursor: pointer;
  background-color: var(--primary-color);
  color: white;
  border: none;
  border-radius: 5px;
}
</style>
