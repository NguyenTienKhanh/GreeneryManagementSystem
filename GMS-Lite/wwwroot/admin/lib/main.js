//<---Controller--->//

$(function () {
    getData();  
})

//<---model--->//

//data
var items = [];
var units = [];
var employees = [];
var itemsMap = [];

//class
function Info(id, userId, submitDate, tree, glass) {
    this.id = id;
    this.userId = userId;
    this.submitDate = submitDate;
    this.tree = tree;
    this.glass = glass;
}
function Tree(name, family, type, height, width, status, lat, lng, location) {
    this.name = name;
    this.family = family;
    this.type = type;
    this.height = height;
    this.width = width;
    this.status = status;
    this.lat = lat;
    this.lng = lng;
    this.location = location;
}
function Glass(name, family, type, surfaceArea, status, polygon, location) {
    this.name = name;
    this.family = family;
    this.type = type;
    this.surfaceArea = surfaceArea;
    this.polygon = polygon;
    this.status = status;
    this.location = location;
}
function Polygon(coords) {
    this.coords = coords;
}
function Coord(lat, lng) {
    this.lat = lat;
    this.lng = lng;
}
function Location(pro, dis, war, uni, sec) {
    this.pro = pro;
    this.dis = dis;
    this.war = war;
    this.uni = uni;
    this.sec = sec;
}
function Employee(id, name) {
    this.id = id;
    this.name = name;
}
function Unit(id, name) {
    this.id = id;
    this.name = name;
}
function User(name, email, phone) {
    this.name = name;
    this.email = email;
    this.phone = phone;
}
//repository
function getData() {   
    $.getJSON("/api/Registration", function (data) {    
        $.each(data, function (index, value) {
            var info = null;
            if (value.regisTree != null) {
                var location = new Location(
                    value.regisTree.location.provinceId,
                    value.regisTree.location.districtId,
                    value.regisTree.location.wardId,
                    value.regisTree.location.unitId,
                    value.regisTree.location.sectionId
                );
                var tree = new Tree(
                    value.regisTree.name,
                    value.regisTree.family,
                    value.regisTree.type,
                    value.regisTree.height,
                    value.regisTree.width,
                    value.regisTree.status,
                    value.regisTree.coord.latitude,
                    value.regisTree.coord.longtitude,
                    location
                );
                info = new Info(value.id, value.userId, value.submitDate, tree, null);
            }
            else {
                var location = new Location(
                    value.regisGlass.location.provinceId,
                    value.regisGlass.location.districtId,
                    value.regisGlass.location.wardId,
                    value.regisGlass.location.unitId,
                    value.regisGlass.location.sectionId
                );
                var glass = new Glass(
                    value.regisGlass.name,
                    value.regisGlass.family,
                    value.regisGlass.type,
                    value.regisGlass.surfaceArea,
                    value.regisGlass.status,
                    value.regisGlass.polygon,
                    location
                );
                info = new Info(value.id, value.userId, value.submitDate, null, glass);
            }
            items.push(info);
            itemsMap[info.id] = info;
        });
        requests(items);
    });
    $.getJSON("/api/Employee", function (data) {
        $.each(data, function (index, value) {
            employees.push(new Employee(value.id, value.name));
        });
    });
    $.getJSON("/api/Unit", function (data) {
        $.each(data, function (index, value) {
            units.push(new Unit(value.id, value.name));
        });
    });
}

//<---Service-->//

//service
function requests(list) {
    var i = 1;
    var html = list.map(info => `
            <tr id="${info.id}">
                <td><a href="#" title="${info.id}" data-toggle="popover" data-trigger="hover" data-placement="top" data-content="">${i++}</a></td>
                <td><input type="button" class="btn btn-primary" value="Người gửi" data-toggle="modal" data-target="#infoModal" onclick="sender('${info.userId}')"></td>
                <td style="color: red">${info.submitDate}</td>
                <td><input type="button" class="btn btn-info" value="Chi Tiết" data-toggle="modal" data-target="#infoModal" onclick="detail('${info.id}')"></td>
                <td>
                    <input type="button" class="btn btn-success" value="Xét Duyệt" data-toggle="modal" data-target="#infoModal" onclick="approvement('${info.id}')">
                    <input type="button" class="btn btn-danger" value="Hủy" onclick="cancel('${info.id}')">
                </td>
            </tr>
`).join('');
    $('tbody').html(html);
}
function approvement(id) {
   //data and views
   var employeeOptions = employees.map(e => `<option value="${e.id}">${e.name}</option>`).join('');
    var unitOptions = units.map(u => `<option value="${u.id}">${u.name}</option>`).join('');
   var html =  `
      <h5>Thiết lập chỉ định quản lý cho cây trồng</h5>
      <form>
        <div class="form-group">
          <label>Nhân viên chịu trách nhiệm quản lý</label>
          <select class="form-control" id="employees">`
            + employeeOptions+`
          </select>
          <br>
          <label>Đơn vị giám sát</label>
          <select class="form-control" id="units">`
            + unitOptions+`
          </select>
        </div>
      </form>
`;
    $('.modal-footer').prepend('<button type="button" class="btn btn-success" data-dismiss="modal" id="approve">Thông Qua</button>');
    $('.modal-title').text('Hoàn Tất Phê Duyệt');
    $('.modal-body').html(html);
    //actions
    $('#approve').click(function () {
        if (confirm('Xác nhận phê chuẩn ?')) {
            var treeData = itemsMap[id].tree;
            var glassData = itemsMap[id].glass;
            var object = null;
            var url = '/api/Tree';
            if (treeData == null) {
                url = '/api/Glass';
                object = {
                    name: glassData.name,
                    family: glassData.family,
                    status: glassData.status,
                    regisDate: new Date(),
                    employeeId: $('#employees').val(),
                    polygon: glassData.polygon,
                    surfaceArea: glassData.surfaceArea,
                    location: {
                        provinceId: glassData.location.pro,
                        districtId: glassData.location.dis,
                        wardId: glassData.location.war,
                        unitId: glassData.location.uni,
                        sectionId: glassData.location.sec
                    },
                    type: glassData.type,
                    managedUnitId: $('#units').val()
                };
            }
            else {
                object = {
                    name: treeData.name,
                    family: treeData.family,
                    height: treeData.height,
                    width: treeData.width,
                    status: treeData.status,
                    regisDate: new Date(),
                    coord: {
                        latitude: treeData.lat,
                        longtitude: treeData.lng
                    },
                    employeeId: $('#employees').val(),
                    location: {
                        provinceId: treeData.location.pro,
                        districtId: treeData.location.dis,
                        wardId: treeData.location.war,
                        unitId: treeData.location.uni,
                        sectionId: treeData.location.sec
                    },
                    type: treeData.type,
                    managedUnitId: $('#units').val()
                };
            }
            var jsonObject = JSON.stringify(object);
            $.ajax({
                type: 'POST',
                url: url,
                data: jsonObject,
                success: function (data) {
                    alert('Phê duyệt đăng ký thành công, thông tin đã được lưu vào cơ sỡ dữ liệu !.');
                    alert('Mã cây: '+data);
                    $.ajax({
                        type: 'DELETE',
                        url: '/api/Registration/' + id,
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            alert('Xảy ra lỗi, liên hệ team phát triển !.');
                            alert(XMLHttpRequest.responseText);
                            alert(textStatus);
                            alert(errorThrown);
                        },
                        success: function () {
                            $('#' + id).remove();
                        }
                    });
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    alert('Xảy ra lỗi, liên hệ team phát triển !.');
                    alert(XMLHttpRequest.responseText);
                    alert(textStatus);
                    alert(errorThrown);
                },
                contentType: "application/json",
            });
        } 
    });
    $('#infoModal').on('hidden.bs.modal', function () {
        $('#approve').remove();
        $('.modal-title').text('Thông Tin Cây');
    });
}
function detail(id) {
    var plant = itemsMap[id].tree != null ? itemsMap[id].tree : itemsMap[id].glass;
    var html = `
            <table style="width: 100%">
                    <tr style="border-bottom: 1px solid">
                        <td style="text-align: right"><b>Tên : </b></td>
                        <td>${plant.name}</td>
                        <td><td>
                        <td><td>
                    </tr>
                    <tr style="border-bottom: 1px solid">
                        <td style="text-align: right"><b>Họ : </b></td>
                        <td>${plant.family}</td>
                        <td><td>
                        <td><td>
                    </tr>
                    <tr style="border-bottom: 1px solid">
                        <td style="text-align: right"><b>Nhóm : </b></td>
                        <td>${plant.type}</td>
                        <td><td>
                        <td><td>
                    </tr>
                    <tr style="border-bottom: 1px solid">
                        <td style="text-align: right"><b>Chiều cao : </b></td>
                        <td>${plant.height}</td>
                        <td style="text-align: right"><b>Đường kính : </b></td>
                        <td>${plant.width}</td>
                    </tr>
                    <tr style="border-bottom: 1px solid">
                        <td style="text-align: right"><b>Tình trạng : </b></td>
                        <td>${plant.status}</td>
                        <td><td>
                        <td><td>
                    </tr>
                    <tr style="border-bottom: 1px solid">
                        <td style="text-align: right"><b>Vĩ độ : </b></td>
                        <td>${plant.lat}</td>
                        <td style="text-align: right"><b>Kinh độ : </b></td>
                        <td>${plant.lng}</td>
                    </tr>
                    <tr>
                        <td style="text-align: right"><b>Phân vùng : </b></td>
                        <td>${plant.location.sec}</td>
                        <td style="text-align: right"><b>Đơn vị : </b></td>
                        <td>${plant.location.uni}</td>
                    </tr>
                </table>
`;
    $('.modal-body').html(html);
}
function cancel(id) {
    if (confirm('Xác nhận hủy ?')) {
        $.ajax({
            type: 'DELETE',
            url: '/api/Registration/' + id,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert('Xảy ra lỗi, liên hệ team phát triển !.');
                alert(XMLHttpRequest.responseText);
                alert(textStatus);
                alert(errorThrown);
            },
            success: function () {
                $('#' + id).remove();
            }
        });
    } 
}
function sender(id) {
    $('.modal-title').text('Thông Tin Người Dùng');
    $.getJSON("/Account/" + id, function (data) {
        var html = `
        <label style="font-weight: bold">Mã: </label>
        <span>${id}</span><br>
        <label style="font-weight: bold">Tên tài khoản: </label>
        <span>${data.userName}</span><br>
        <label style="font-weight: bold">Email: </label>
        <span>${data.email}</span><br>
        <label style="font-weight: bold">Số điện thoại: </label>
        <span>${data.phone}</span>  
    `;
        $('.modal-body').html(html);
    });
    $('#infoModal').on('hidden.bs.modal', function () {
        $('.modal-title').text('Thông Tin Cây');
    });

}
//template