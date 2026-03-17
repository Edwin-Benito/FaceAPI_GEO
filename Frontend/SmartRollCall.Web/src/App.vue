<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from "vue";
import { LoaderOptions } from "@googlemaps/js-api-loader";

// --- ESTADO GLOBAL ---
const SALONES = ref<any[]>([]);
const aulaSeleccionada = ref<any>(null);
const historial = ref<any[]>([]);
const status = ref("GOOGLE CLOUD ONLINE");
const detectionStatus = ref("ESPERANDO CAPTURA");
const modoSimulado = ref(true); // Interruptor para pruebas

// Coordenadas y GPS
const coordsActuales = ref({ lat: 19.9801, lng: -98.6856 });
const metricaGPS = ref(0);

// Refs del DOM
const video = ref<HTMLVideoElement | null>(null);
const canvas = ref<HTMLCanvasElement | null>(null);

// Variables de Google Maps
let map: google.maps.Map | null = null;
let userMarker: google.maps.marker.AdvancedMarkerElement | null = null;
let zonasAula: google.maps.Circle[] = []; // Array para guardar todos los círculos

// --- LÓGICA DE GOOGLE MAPS ---
const initMap = async () => {
  const { setOptions, importLibrary } = await import("@googlemaps/js-api-loader");

  // Configuramos la API
  setOptions({
    apiKey: import.meta.env.VITE_GOOGLE_MAPS_API_KEY as string,
    version: "weekly",
  });

  // Extraemos las clases nativas
  const { Map, Circle } = (await importLibrary("maps")) as google.maps.MapsLibrary;
  const { AdvancedMarkerElement } = (await importLibrary("marker")) as google.maps.MarkerLibrary;

  const mapOptions = {
    center: coordsActuales.value,
    zoom: 19, // Aumenté el zoom para que se vea más de cerca la facultad
    mapTypeId: "satellite", // <--- Esta es la línea clave para la vista de satélite
    mapId: "DEMO_MAP_ID", // Requerido para usar AdvancedMarkers
    disableDefaultUI: true, // Mantiene la interfaz limpia sin botones molestos
  };

  const mapDiv = document.getElementById("map-container") as HTMLElement;
  map = new Map(mapDiv, mapOptions);

  userMarker = new AdvancedMarkerElement({
    position: coordsActuales.value,
    map: map,
    title: "Tu ubicación",
  });

  // Dibujar todas las zonas permitidas (Salones)
  SALONES.value.forEach(salon => {
    const circle = new Circle({
      map: map,
      center: { lat: salon.latitude, lng: salon.longitude },
      radius: 15, // Radio de 15 metros de tolerancia
      fillColor: "#10b981", // Color Verde esmeralda para zonas permitidas
      fillOpacity: 0.2,
      strokeColor: "#10b981",
      strokeWeight: 2,
    });
    
    // Label clickeable o title (Google Maps Circle no tiene Title nativo, pero visualmente el fill basta)
    zonasAula.push(circle);
  });
};

const actualizarMapa = (lat: number, lng: number) => {
  if (!map || !userMarker) return;
  const newPos = { lat, lng };
  map.setCenter(newPos);
  userMarker.position = newPos;
};

const gestionarUbicacion = () => {
  if (modoSimulado.value && aulaSeleccionada.value) {
    coordsActuales.value = {
      lat: aulaSeleccionada.value.latitude,
      lng: aulaSeleccionada.value.longitude,
    };
    actualizarMapa(coordsActuales.value.lat, coordsActuales.value.lng);
  } else {
    navigator.geolocation.getCurrentPosition((pos) => {
      coordsActuales.value = {
        lat: pos.coords.latitude,
        lng: pos.coords.longitude,
      };
      actualizarMapa(coordsActuales.value.lat, coordsActuales.value.lng);
    });
  }
};

// --- API FETCH ---
const fetchSalones = async () => {
  try {
    const res = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:5177'}/api/ClassGroups`);
    if (res.ok) {
      SALONES.value = await res.json();
      if (SALONES.value.length > 0) aulaSeleccionada.value = SALONES.value[0];
    }
  } catch (e) {
    status.value = "Error: Backend Offline";
  }
};

const fetchHistory = async () => {
  try {
    const res = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:5177'}/api/RollCall/history`);
    if (res.ok) historial.value = await res.json();
  } catch (e) {
    console.error(e);
  }
};

const procesarAsistencia = async () => {
  if (!aulaSeleccionada.value || !video.value || !canvas.value) return;

  detectionStatus.value = "SCANNING...";
  const context = canvas.value.getContext("2d");
  context?.drawImage(video.value, 0, 0, 640, 480);
  const foto = canvas.value.toDataURL("image/jpeg").split(",")[1];

  try {
    const res = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:5177'}/api/RollCall/process`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        classGroupId: aulaSeleccionada.value.id,
        photoBase64: foto,
        latitude: coordsActuales.value.lat,
        longitude: coordsActuales.value.lng,
      }),
    });

    const data = await res.json();
    if (res.ok) {
      detectionStatus.value = "DETECTADO";
      metricaGPS.value = data.distancia;
      fetchHistory();
    } else {
      detectionStatus.value = data.message;
    }
  } catch (e) {
    status.value = "Error de red";
  }
};

// --- INICIO ---
onMounted(async () => {
  await fetchSalones();
  await fetchHistory();

  // Cargamos la cámara
  await nextTick();
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ video: true });
    if (video.value) video.value.srcObject = stream;
  } catch (err) {
    status.value = "Cámara no disponible";
  }

  // Inicializar Google Maps
  initMap();
  gestionarUbicacion();
});

watch(aulaSeleccionada, () => {
  if (modoSimulado.value) gestionarUbicacion();
});
</script>

<template>
  <div
    class="min-h-screen bg-[#0f172a] p-4 sm:p-6 text-slate-200 font-sans italic selection:bg-indigo-500"
  >
    <div class="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-6 sm:gap-8">
      <div class="lg:col-span-4 space-y-4 sm:space-y-6">
        <div
          class="bg-slate-800/40 p-6 sm:p-8 rounded-[30px] sm:rounded-[40px] border border-slate-700 shadow-2xl"
        >
          <h1
            class="text-indigo-400 font-black text-xl mb-6 uppercase tracking-widest italic"
          >
            Intelligent Pass
          </h1>

          <div
            class="flex items-center justify-between bg-slate-900/50 p-4 rounded-2xl mb-6 border border-slate-800"
          >
            <span class="text-[9px] font-black uppercase tracking-widest"
              >Modo Simulado (GPS OK)</span
            >
            <input
              type="checkbox"
              v-model="modoSimulado"
              @change="gestionarUbicacion"
              class="toggle-checkbox"
            />
          </div>

          <select
            v-model="aulaSeleccionada"
            class="w-full bg-slate-900 p-4 rounded-2xl border border-slate-700 text-indigo-300 font-bold mb-6 outline-none appearance-none cursor-pointer truncate pr-10"
            style="background-image: url('data:image/svg+xml;charset=US-ASCII,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%22292.4%22%20height%3D%22292.4%22%3E%3Cpath%20fill%3D%22%23818cf8%22%20d%3D%22M287%2069.4a17.6%2017.6%200%200%200-13-5.4H18.4c-5%200-9.3%201.8-12.9%205.4A17.6%2017.6%200%200%200%200%2082.2c0%205%201.8%209.3%205.4%2012.9l128%20127.9c3.6%203.6%207.8%205.4%2012.8%205.4s9.2-1.8%2012.8-5.4L287%2095c3.5-3.5%205.4-7.8%205.4-12.8%200-5-1.9-9.2-5.5-12.8z%22%2F%3E%3C%2Fsvg%3E'); background-repeat: no-repeat; background-position: right 1rem top 50%; background-size: 0.65rem auto;"
          >
            <option v-for="s in SALONES" :key="s.id" :value="s" class="bg-slate-900 text-slate-200">
              {{ s.groupName }}
            </option>
          </select>

          <div
            class="relative rounded-[30px] overflow-hidden border-2 border-slate-700 aspect-square mb-6 bg-black"
          >
            <video
              ref="video"
              autoplay
              class="w-full h-full object-cover opacity-80"
            ></video>
            <canvas
              ref="canvas"
              width="640"
              height="480"
              class="hidden"
            ></canvas>
            <div
              class="absolute inset-0 flex items-center justify-center pointer-events-none"
            >
              <div
                class="w-40 h-40 border border-indigo-500/20 rounded-full animate-ping"
              ></div>
            </div>
          </div>

          <button
            @click="procesarAsistencia"
            class="w-full py-5 bg-indigo-600 hover:bg-indigo-500 text-white font-black rounded-3xl shadow-xl transition-all active:scale-95 uppercase text-[10px] tracking-[0.3em]"
          >
            Pasar Asistencia
          </button>
          <p
            class="text-center mt-4 text-[9px] text-slate-500 uppercase font-black"
          >
            {{ status }}
          </p>
        </div>
      </div>

      <div class="lg:col-span-8 space-y-4 sm:space-y-6">
        <div class="grid grid-cols-3 gap-2 sm:gap-4">
          <div
            class="bg-slate-800/40 p-3 sm:p-5 rounded-[20px] sm:rounded-[30px] border border-slate-700 text-center flex flex-col justify-center"
          >
            <p class="text-[8px] text-slate-500 font-bold uppercase mb-1">
              Human Detection
            </p>
            <p
              class="text-xs font-black"
              :class="
                detectionStatus === 'DETECTED'
                  ? 'text-green-400'
                  : 'text-slate-400'
              "
            >
              {{ detectionStatus }}
            </p>
          </div>
          <div
            class="bg-slate-800/40 p-5 rounded-[30px] border border-slate-700 text-center"
          >
            <p class="text-[8px] text-slate-500 font-bold uppercase mb-1">
              Distance
            </p>
            <p class="text-lg font-black text-indigo-400">{{ metricaGPS }}m</p>
          </div>
          <div
            class="bg-slate-800/40 p-5 rounded-[30px] border border-slate-700 text-center"
          >
            <p class="text-[8px] text-slate-500 font-bold uppercase mb-1">
              Coords
            </p>
            <p class="text-[8px] font-mono text-slate-400">
              {{ coordsActuales.lat.toFixed(5) }},
              {{ coordsActuales.lng.toFixed(5) }}
            </p>
          </div>
        </div>

        <div
          id="map-container"
          class="h-[250px] sm:h-[350px] rounded-[30px] sm:rounded-[40px] border-2 border-slate-700 shadow-2xl overflow-hidden"
        ></div>

        <div
          class="bg-slate-800/20 rounded-[25px] sm:rounded-[35px] border border-slate-800 overflow-hidden backdrop-blur-sm overflow-x-auto"
        >
          <table class="w-full text-[10px]">
            <thead class="bg-slate-900/50 text-slate-500 uppercase font-black">
              <tr>
                <th class="p-4 text-left">Hora</th>
                <th class="p-4 text-left">Identidad</th>
                <th class="p-4 text-right">Aula</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-800/50">
              <tr v-if="historial.length === 0">
                <td
                  colspan="3"
                  class="p-8 text-center text-slate-600 uppercase font-black tracking-widest"
                >
                  No hay pases de lista hoy
                </td>
              </tr>
              <tr
                v-for="log in historial"
                :key="log.hora"
                class="hover:bg-indigo-500/5 transition-colors"
              >
                <td class="p-4 font-mono text-slate-500">{{ log.hora }}</td>
                <td
                  class="p-4 font-bold text-white italic underline decoration-indigo-500/20"
                >
                  {{ log.usuario }}
                </td>
                <td class="p-4 text-right font-black text-green-400">
                  {{ log.aula }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
.toggle-checkbox {
  width: 40px;
  height: 20px;
  background: #334155;
  border-radius: 99px;
  appearance: none;
  cursor: pointer;
  position: relative;
  transition: all 0.3s;
}
.toggle-checkbox:checked {
  background: #6366f1;
}
.toggle-checkbox::after {
  content: "";
  position: absolute;
  width: 16px;
  height: 16px;
  top: 2px;
  left: 2px;
  background: white;
  border-radius: 50%;
  transition: all 0.3s;
}
.toggle-checkbox:checked::after {
  left: 22px;
}
</style>
