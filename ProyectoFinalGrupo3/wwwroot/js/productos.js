$(document).ready(function () {

    // Modal Agregar
    const $modalAgregar = $("#modalAgregar");

    $("#btnAgregar").on("click", function () {
        $modalAgregar.removeClass("hidden");
    });

    $("#btnCerrarModal, #btnCancelarModal").on("click", function () {
        $modalAgregar.addClass("hidden");
    });

    $modalAgregar.on("click", function (e) {
        if (e.target === this) {
            $modalAgregar.addClass("hidden");
        }
    });

    // Modal Editar
    $(document).on("click", ".btnEditar", function () {

        console.log("CLICK EDITAR");

        let codigo = $(this).data("codigo");
        let nombre = $(this).data("nombre");
        let categoria = $(this).data("categoria");
        let precio = $(this).data("precio");
        let cantidad = $(this).data("cantidad");
        let empaque = $(this).data("empaque");
        let costoEmpaque = $(this).data("costoempaque");
        let estado = $(this).data("estado");
        let imagen = $(this).data("imagen");


        $("#editCodigoProducto").val(codigo);
        $("#editNombre").val(nombre);
        $("#editCategoria").val(categoria); // debe ser el ID de la categoría
        $("#editPrecio").val(precio);
        $("#editCantidad").val(cantidad);
        $("#editEmpaque").val(empaque);
        $("#editCostoEmpaque").val(costoEmpaque);
        $("#editEstado").val(estado);
        $("#editUrlImagen").val(imagen);

        $("#modalEditar").removeClass("hidden");
    });

    $("#cerrarEditar, #cancelarEditar").on("click", function () {
        $("#modalEditar").addClass("hidden");
    });

    $("#modalEditar").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });

    // Modal Eliminar
    const modalEliminar = $("#modalEliminar");

    $(document).on("click", ".btnEliminar", function () {
        let id = $(this).data("id");

        $("#idEliminar").val(id);
        modalEliminar.removeClass("hidden");
    });

    $("#cancelarEliminar").on("click", function () {
        modalEliminar.addClass("hidden");
    });

    modalEliminar.on("click", function (e) {
        if (e.target === this) {
            modalEliminar.addClass("hidden");
        }
    });

    // Modal Ver
    $(document).on("click", ".btnVer", function () {

        let codigo = $(this).data("codigo");
        let nombre = $(this).data("nombre");
        let categoria = $(this).data("categoria");
        let precio = $(this).data("precio");
        let cantidad = $(this).data("cantidad");
        let empaque = $(this).data("empaque");
        let costoEmpaque = $(this).data("costoempaque");
        let estado = $(this).data("estado");
        let imagen = $(this).data("imagen");

        $("#verCodigoProducto").text("PROD#" + codigo);
        $("#verNombreProducto").text(nombre);
        $("#verCategoriaProducto").text(categoria);
        $("#verPrecioProducto").text("₡" + precio);
        $("#verCantidadProducto").text(cantidad);
        $("#verEmpaqueProducto").text(empaque == 1 ? "Sí" : "No");
        $("#verCostoEmpaqueProducto").text("₡" + costoEmpaque);
        $("#verEstadoProducto").text(estado == 1 ? "Activo" : "Inactivo");
        $("#verImagenProducto").attr("src", imagen);

        $("#modalVer").removeClass("hidden");
    });

    $("#cerrarVer, #cerrarVerBtn").on("click", function () {
        $("#modalVer").addClass("hidden");
    });

    $("#modalVer").on("click", function (e) {
        if (e.target === this) {
            $(this).addClass("hidden");
        }
    });
});