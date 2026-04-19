$(document).ready(function () {

    function abrirModal(modalId) {
        $(modalId).removeClass("hidden");
    }

    function cerrarModal(modalId) {
        $(modalId).addClass("hidden");
    }

    function toggleMesaCrear() {
        let tipo = $("#crearTipoPedido").val();

        if (tipo === "Dine-in") {
            $("#contenedorMesaCrear").removeClass("hidden");
            $("#crearNumeroMesa").prop("disabled", false);
        } else {
            $("#contenedorMesaCrear").addClass("hidden");
            $("#crearNumeroMesa").val("");
            $("#crearNumeroMesa").prop("disabled", true);
        }
    }

    function toggleMesaEditar() {
        let tipo = $("#editTipoPedido").val();

        if (tipo === "Dine-in") {
            $("#contenedorMesaEditar").removeClass("hidden");
            $("#editNumeroMesa").prop("disabled", false);
        } else {
            $("#contenedorMesaEditar").addClass("hidden");
            $("#editNumeroMesa").val("");
            $("#editNumeroMesa").prop("disabled", true);
        }
    }

    // MODAL AGREGAR
    $("#btnAgregar").on("click", function () {
        abrirModal("#modalAgregar");
        toggleMesaCrear();
    });

    $("#btnCerrarModal, #btnCancelarModal").on("click", function () {
        cerrarModal("#modalAgregar");
    });

    $("#modalAgregar").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalAgregar");
        }
    });

    // MODAL EDITAR
    $(document).on("click", ".btnEditar", function () {
        let id = $(this).data("id");
        let usuario = $(this).data("usuario");
        let tipoPedido = $(this).data("tipopedido");
        let numeroMesa = $(this).data("numeromesa");
        let estado = $(this).data("estado");

        $("#editCodigoPedido").val(id);
        $("#editIdUsuario").val(usuario);
        $("#editTipoPedido").val(tipoPedido);
        $("#editNumeroMesa").val(numeroMesa);
        $("#editEstado").val(estado);

        toggleMesaEditar();
        abrirModal("#modalEditar");
    });

    $("#cerrarEditar, #cancelarEditar").on("click", function () {
        cerrarModal("#modalEditar");
    });

    $("#modalEditar").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalEditar");
        }
    });

    // MODAL ELIMINAR
    $(document).on("click", ".btnEliminar", function () {
        let id = $(this).data("id");
        $("#idEliminar").val(id);
        abrirModal("#modalEliminar");
    });

    $("#cancelarEliminar").on("click", function () {
        cerrarModal("#modalEliminar");
    });

    $("#modalEliminar").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalEliminar");
        }
    });

    // MODAL VER
    $(document).on("click", ".btnVer", function () {
        let id = $(this).data("id");
        let usuario = $(this).data("usuario");
        let tipoPedido = $(this).data("tipopedido");
        let numeroMesa = $(this).data("numeromesa");
        let estado = $(this).data("estado");
        let fecha = $(this).data("fecha");

        $("#verCodigoPedido").text("#ORD-" + id);
        $("#verIdUsuario").text(usuario || "Sin usuario");
        $("#verTipoPedido").text(tipoPedido || "Sin tipo");
        $("#verNumeroMesa").text(numeroMesa ? "Mesa " + numeroMesa : "No aplica");
        $("#verEstado").text(estado || "Sin estado");
        $("#verFecha").text(fecha || "Sin fecha");

        abrirModal("#modalVer");
    });

    $("#cerrarVer, #btnCerrarVer").on("click", function () {
        cerrarModal("#modalVer");
    });

    $("#modalVer").on("click", function (e) {
        if (e.target === this) {
            cerrarModal("#modalVer");
        }
    });

    // CAMBIO DE TIPO DE PEDIDO
    $("#crearTipoPedido").on("change", function () {
        toggleMesaCrear();
    });

    $("#editTipoPedido").on("change", function () {
        toggleMesaEditar();
    });

    toggleMesaCrear();
    toggleMesaEditar();

    // AGREGAR FILA DE PRODUCTO
    $("#btnAgregarFilaProducto").on("click", function () {
        let nuevaFila = $("#contenedorProductos .fila-producto:first").clone();

        nuevaFila.find("select").val("");
        nuevaFila.find("input[name='cantidades']").val("1");

        $("#contenedorProductos").append(nuevaFila);
    });

    // QUITAR FILA DE PRODUCTO
    $(document).on("click", ".btnQuitarProducto", function () {
        if ($("#contenedorProductos .fila-producto").length > 1) {
            $(this).closest(".fila-producto").remove();
        }
    });

});
