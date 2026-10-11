class Car
{
    public string Model;
    public string Number;
    public bool IsBroken;
    public bool IsBusy;

    public Car(string model, string number)
    {
        Model = model;
        Number = number;
        IsBroken = false;
        IsBusy = false;
    }

    public void Show()
    {
        string state = IsBroken ? "неисправен" : "исправен";
        string busy = IsBusy ? "на рейсе" : "свободен";
        Console.WriteLine($"Авто: {Model} {Number}, {state}, {busy}");
    }
}

class Trip
{
    public int Id;
    public string Route;
    public Driver Driver;
    public Car Car;
    public bool IsDone;
    public string Report;

    public Trip(int id, string route)
    {
        Id = id;
        Route = route;
        IsDone = false;
    }

    public void Show()
    {
        string driverName = Driver == null ? "не назначен" : Driver.Name;
        string carNumber = Car == null ? "не назначен" : Car.Number;
        string status = IsDone ? "выполнен" : "не выполнен";
        Console.WriteLine($"Рейс {Id} ({Route}), водитель: {driverName}, авто: {carNumber}, {status}");
    }
}

class RepairRequest
{
    public Driver Driver;
    public Car Car;
    public string Description;
    public bool IsDone;

    public RepairRequest(Driver driver, Car car, string description)
    {
        Driver = driver;
        Car = car;
        Description = description;
        IsDone = false;
    }
}

class Driver
{
    public string Name;
    public bool IsSuspended;
    public Trip CurrentTrip;

    public Driver(string name)
    {
        Name = name;
        IsSuspended = false;
        CurrentTrip = null;
    }

    public void SendRepairRequest(Car car, string description, Dispatcher dispatcher)
    {
        if (IsSuspended)
        {
            Console.WriteLine($"Водитель {Name} отстранён, заявку подать нельзя");
            return;
        }

        RepairRequest request = new RepairRequest(this, car, description);
        dispatcher.Requests.Add(request);
        car.IsBroken = true;
        Console.WriteLine($"Водитель {Name} подал заявку на ремонт {car.Number}: {description}");
    }

    public void FinishTrip(bool carIsBroken, string report)
    {
        if (CurrentTrip == null)
        {
            Console.WriteLine($"У водителя {Name} нет рейса");
            return;
        }

        CurrentTrip.IsDone = true;
        CurrentTrip.Report = report;
        CurrentTrip.Car.IsBroken = carIsBroken;
        CurrentTrip.Car.IsBusy = false;
        Console.WriteLine($"Водитель {Name} выполнил рейс {CurrentTrip.Id}. Отчёт: {report}");
        CurrentTrip = null;
    }

    public void Show()
    {
        string status = IsSuspended ? "отстранён" : "работает";
        Console.WriteLine($"Водитель: {Name}, {status}");
    }
}

class Dispatcher
{
    public string Name;
    public List<RepairRequest> Requests = new List<RepairRequest>();

    public Dispatcher(string name)
    {
        Name = name;
    }

    public void AssignTrip(Trip trip, Driver driver, Car car)
    {
        if (trip.IsDone || trip.Driver != null)
        {
            Console.WriteLine($"Рейс {trip.Id} уже назначен или выполнен");
            return;
        }
        if (driver.IsSuspended)
        {
            Console.WriteLine($"Водитель {driver.Name} отстранён");
            return;
        }
        if (driver.CurrentTrip != null)
        {
            Console.WriteLine($"Водитель {driver.Name} уже занят");
            return;
        }
        if (car.IsBroken)
        {
            Console.WriteLine($"Авто {car.Number} неисправно");
            return;
        }
        if (car.IsBusy)
        {
            Console.WriteLine($"Авто {car.Number} занято");
            return;
        }

        trip.Driver = driver;
        trip.Car = car;
        driver.CurrentTrip = trip;
        car.IsBusy = true;
        Console.WriteLine($"Диспетчер назначил рейс {trip.Id} водителю {driver.Name} на авто {car.Number}");
    }

    public void SuspendDriver(Driver driver)
    {
        driver.IsSuspended = true;
        Console.WriteLine($"Диспетчер отстранил водителя {driver.Name}");

        if (driver.CurrentTrip != null)
        {
            driver.CurrentTrip.Car.IsBusy = false;
            driver.CurrentTrip.Car = null;
            driver.CurrentTrip.Driver = null;
            driver.CurrentTrip = null;
            Console.WriteLine("Рейс снят с водителя и ждёт нового назначения");
        }
    }

    public void RepairCar(RepairRequest request)
    {
        request.Car.IsBroken = false;
        request.IsDone = true;
        Console.WriteLine($"Авто {request.Car.Number} отремонтировано");
    }
}

class Program
{
    static void Main()
    {
        Dispatcher dispatcher = new Dispatcher("Иванов");

        Driver driver1 = new Driver("Петров");
        Driver driver2 = new Driver("Сидоров");

        Car car1 = new Car("КАМАЗ", "А111АА");
        Car car2 = new Car("ГАЗель", "В222ВВ");

        Trip trip1 = new Trip(1, "Астана - Караганда");
        Trip trip2 = new Trip(2, "Астана - Кокшетау");

        dispatcher.AssignTrip(trip1, driver1, car1);
        dispatcher.AssignTrip(trip2, driver2, car2);

        driver1.FinishTrip(true, "Стук в подвеске");
        driver1.SendRepairRequest(car1, "Замена амортизаторов", dispatcher);

        dispatcher.SuspendDriver(driver2);
        dispatcher.AssignTrip(trip2, driver2, car2);

        dispatcher.RepairCar(dispatcher.Requests[0]);
        dispatcher.AssignTrip(trip2, driver1, car1);
        driver1.FinishTrip(false, "Всё хорошо");

        Console.WriteLine();
        driver1.Show();
        driver2.Show();
        car1.Show();
        car2.Show();
        trip1.Show();
        trip2.Show();
    }
}