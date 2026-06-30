// =============================================================
// LÓGICA GLOBAL DE NAVEGACIÓN (site.js)
// =============================================================

document.addEventListener("DOMContentLoaded", () => {

    // 1. AUTO-FOCUS PARA TODOS LOS MODALES
    // Cada vez que se abre un modal, busca el primer campo y le pone el foco.
    document.body.addEventListener('shown.bs.modal', function (event) {
        const modal = event.target;
        // Busca el primer input/select que no esté oculto ni deshabilitado
        const primerCampo = modal.querySelector('input:not([type=hidden]):not([disabled]):not([readonly]), select:not([disabled]), textarea:not([disabled])');
        if (primerCampo) {
            primerCampo.focus();
            // Si es texto, lo selecciona todo para que puedas sobreescribir rápido
            if (primerCampo.select) primerCampo.select(); 
        }
    }, true); 

    // 2. TECLA ENTER INTELIGENTE
    // Permite disparar funciones específicas tocando Enter en cualquier input
    document.body.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            const input = e.target;
            
            // Si el input tiene el atributo data-enter="NombreFuncion"
            if (input.dataset && input.dataset.enter) {
                e.preventDefault(); // Evita el submit del form
                const nombreFuncion = input.dataset.enter;

                // Busca la función globalmente y la ejecuta
                if (typeof window[nombreFuncion] === 'function') {
                    window[nombreFuncion]();
                } else {
                    console.warn(`La función '${nombreFuncion}' no existe o no es global.`);
                }
            }
        }
    });
});